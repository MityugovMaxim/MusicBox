using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIAudioWave : Graphic, IDisposable
{
	public AudioClip AudioClip
	{
		get => m_AudioClip;
		set
		{
			if (m_AudioClip == value)
				return;
			
			m_AudioClip = value;
			
			Render();
		}
	}

	public double Time
	{
		get => m_Time;
		set
		{
			if (Math.Abs(m_Time - value) < double.Epsilon)
				return;
			
			m_Time = value;
			
			ProcessTime();
		}
	}

	public int Frequency => m_Frequency;

	public float Duration
	{
		get => m_Duration;
		set
		{
			if (Mathf.Approximately(m_Duration, value))
				return;
			
			m_Duration = value;
			
			m_MinTime = m_Duration * (Ratio - 1);
			m_MaxTime = m_Duration * Ratio;
			
			ProcessLimits();
		}
	}

	public float Ratio
	{
		get => m_Ratio;
		set
		{
			if (Mathf.Approximately(m_Ratio, value))
				return;
			
			m_Ratio = value;
			
			ProcessLimits();
		}
	}

	public override Material defaultMaterial
	{
		get
		{
			if (m_DefaultMaterial == null)
			{
				m_DefaultMaterial           = new Material(Shader.Find("UI/AudioWave"));
				m_DefaultMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_DefaultMaterial;
		}
	}

	static Material m_DefaultMaterial;

	static readonly int m_BackgroundColorPropertyID  = Shader.PropertyToID("_BackgroundColor");
	static readonly int m_MaxColorPropertyID         = Shader.PropertyToID("_MaxColor");
	static readonly int m_MaxSamplesPropertyID       = Shader.PropertyToID("_MaxSamples");
	static readonly int m_MaxSamplesLengthPropertyID = Shader.PropertyToID("_MaxSamplesLength");
	static readonly int m_AvgColorPropertyID         = Shader.PropertyToID("_AvgColor");
	static readonly int m_AvgSamplesPropertyID       = Shader.PropertyToID("_AvgSamples");
	static readonly int m_AvgSamplesLengthPropertyID = Shader.PropertyToID("_AvgSamplesLength");
	static readonly int m_MinTimePropertyID          = Shader.PropertyToID("_MinTime");
	static readonly int m_MaxTimePropertyID          = Shader.PropertyToID("_MaxTime");
	static readonly int m_SizePropertyID             = Shader.PropertyToID("_Size");

	[SerializeField] Color     m_BackgroundColor = new Color(0.12f, 0.12f, 0.12f, 1);
	[SerializeField] Color     m_MaxColor        = new Color(1, 0.5f, 0, 1);
	[SerializeField] Color     m_AvgColor        = new Color(1, 0.75f, 0, 1);
	[SerializeField] double    m_Time;
	[SerializeField] float     m_Duration;
	[SerializeField] float     m_Ratio;
	[SerializeField] AudioClip m_AudioClip;

	double m_MinTime;
	double m_MaxTime;

	int                     m_Samples;
	int                     m_Frequency;
	double                  m_SamplesPerUnit;
	CancellationTokenSource m_TokenSource;

	[SerializeField] Texture2D m_MaxTexture;
	[SerializeField] Texture2D m_AvgTexture;

	readonly UIVertex[] m_Vertices =
	{
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
	};

	protected override void Awake()
	{
		base.Awake();
		
		ProcessLimits();
		ProcessTime();
		Render();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (this is IDisposable disposable)
			disposable.Dispose();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessTime();
		ProcessColor();
	}
	#endif

	void ProcessTime()
	{
		int minTime = (int)(((m_Time + m_MinTime) * m_Frequency) / m_SamplesPerUnit);
		int maxTime = (int)(((m_Time + m_MaxTime) * m_Frequency) / m_SamplesPerUnit);
		
		material.SetInt(m_MinTimePropertyID, minTime);
		material.SetInt(m_MaxTimePropertyID, maxTime);
	}

	void ProcessLimits()
	{
		m_MinTime = Duration * (Ratio - 1);
		m_MaxTime = Duration * Ratio;
	}

	void ProcessColor()
	{
		material.SetColor(m_BackgroundColorPropertyID, m_BackgroundColor);
		material.SetColor(m_MaxColorPropertyID, m_MaxColor);
		material.SetColor(m_AvgColorPropertyID, m_AvgColor);
	}

	public async Task Render()
	{
		if (m_AudioClip == null || Mathf.Approximately(m_Duration, 0))
			return;
		
		Rect rect = GetPixelAdjustedRect();
		
		m_Frequency      = m_AudioClip.frequency;
		m_Samples        = m_AudioClip.samples;
		m_SamplesPerUnit = m_Frequency / (rect.height / (m_MaxTime - m_MinTime)) * 4;
		
		ProcessTime();
		
		ProcessColor();
		
		await LoadAudioData(m_AudioClip);
	}

	async Task LoadAudioData(AudioClip _AudioClip)
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		Task<float[]> maxDataTask = GetAudioData(_AudioClip, _Buffer => _Buffer.Max(Mathf.Abs), token);
		Task<float[]> avgDataTask = GetAudioData(_AudioClip, _Buffer => _Buffer.Average(Mathf.Abs), token);
		
		await Task.WhenAll(maxDataTask, avgDataTask);
		
		float[] maxData = maxDataTask.Result;
		float[] avgData = avgDataTask.Result;
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		int size = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(maxData.Length, avgData.Length))));
		
		if (size == 0)
			return;
		
		m_MaxTexture            = new Texture2D(size, size, TextureFormat.ARGB32, false);
		m_MaxTexture.filterMode = FilterMode.Point;
		m_AvgTexture            = new Texture2D(size, size, TextureFormat.ARGB32, false);
		m_AvgTexture.filterMode = FilterMode.Point;
		for (int x = 0; x < size; x++)
		for (int y = 0; y < size; y++)
		{
			int index = y * size + x;
			float maxValue = index >= 0 && index < maxData.Length
				? maxData[index]
				: 0;
			float avgValue = index >= 0 && index < avgData.Length
				? avgData[index]
				: 0;
			m_MaxTexture.SetPixel(x, y, new Color(1, 1, 1, maxValue));
			m_AvgTexture.SetPixel(x, y, new Color(1, 1, 1, avgValue));
		}
		m_MaxTexture.Apply();
		m_AvgTexture.Apply();
		
		await Task.Yield();
		
		material.SetInt(m_SizePropertyID, size);
		
		if (maxData.Length > 0)
		{
			material.SetTexture(m_MaxSamplesPropertyID, m_MaxTexture);
			material.SetInt(m_MaxSamplesLengthPropertyID, maxData.Length);
		}
		
		if (avgData.Length > 0)
		{
			material.SetTexture(m_AvgSamplesPropertyID, m_AvgTexture);
			material.SetInt(m_AvgSamplesLengthPropertyID, avgData.Length);
		}
	}

	async Task<float[]> GetAudioData(AudioClip _AudioClip, Func<float[], float> _Function, CancellationToken _Token = default)
	{
		if (_AudioClip == null)
			return Array.Empty<float>();
		
		int samples   = (int)(m_Samples / m_SamplesPerUnit);
		int threshold = samples / 30;
		
		float[] buffer = new float[(int)m_SamplesPerUnit];
		float[] data   = new float[samples];
		
		for (int i = 0; i < data.Length; i++)
		{
			if (_Token.IsCancellationRequested)
				break;
			
			_AudioClip.GetData(buffer, (int)(i * m_SamplesPerUnit));
			
			if (i % threshold == 0)
				await Task.Yield();
			
			data[i] = _Function(buffer);
		}
		
		return data;
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
		
		Rect rect = GetPixelAdjustedRect();
		Color32 color32 = color;
		
		m_Vertices[0].position = new Vector3(rect.xMin, rect.yMin);
		m_Vertices[1].position = new Vector3(rect.xMin, rect.yMax);
		m_Vertices[2].position = new Vector3(rect.xMax, rect.yMax);
		m_Vertices[3].position = new Vector3(rect.xMax, rect.yMin);
		
		m_Vertices[0].uv0 = new Vector2(0, 0);
		m_Vertices[1].uv0 = new Vector2(0, 1);
		m_Vertices[2].uv0 = new Vector2(1, 1);
		m_Vertices[3].uv0 = new Vector2(1, 0);
		
		m_Vertices[0].color = color32;
		m_Vertices[1].color = color32;
		m_Vertices[2].color = color32;
		m_Vertices[3].color = color32;
		
		_VertexHelper.AddUIVertexQuad(m_Vertices);
	}

	void IDisposable.Dispose()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}
