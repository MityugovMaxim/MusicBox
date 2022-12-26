using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UIAudioWave : UIImage
{
	class Block
	{
		public bool Full => m_Count >= m_Data.Length;

		readonly float[] m_Data;

		int m_Count;

		public Block(int _Capacity)
		{
			m_Data = new float[_Capacity];
		}

		public Color Convert() => new Color(GetMax(), GetAverage(), GetRMS(), 1);

		float GetMax()
		{
			float value = 0;
			for (int i = 0; i < m_Count; i++)
				value = Mathf.Max(value, m_Data[i]);
			return value;
		}

		float GetAverage()
		{
			float value = 0;
			for (int i = 0; i < m_Count; i++)
				value += m_Data[i];
			return value / m_Count;
		}

		float GetRMS()
		{
			double value = 0;
			for (int i = 0; i < m_Count; i++)
				value += m_Data[i] * m_Data[i];
			return (float)Math.Sqrt(value / m_Count);
		}

		public void Add(float _Value)
		{
			m_Data[m_Count++] = Mathf.Abs(_Value);
		}

		public void Clear()
		{
			m_Count = 0;
		}
	}

	public enum Mode
	{
		Vertical   = 0,
		Horizontal = 1,
	}

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
			if (Math.Abs(m_Time - value) < double.Epsilon * 2)
				return;
			
			m_Time = value;
			
			SetVerticesDirty();
		}
	}

	public double Duration
	{
		get => m_Duration;
		set
		{
			if (Math.Abs(m_Duration - value) < double.Epsilon * 2)
				return;
			
			m_Duration = value;
			
			SetVerticesDirty();
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
			
			SetVerticesDirty();
		}
	}

	[SerializeField] double    m_Time;
	[SerializeField] double    m_Duration;
	[SerializeField] float     m_Ratio;
	[SerializeField] AudioClip m_AudioClip;
	[SerializeField] Mode      m_Mode;

	[SerializeField, Range(0, 1)] float m_Quality = 1; 

	CancellationTokenSource m_TokenSource;

	protected override Material GetMaterial()
	{
		return CreateMaterial("UI/AudioWave");
	}

	float GetSize()
	{
		Rect rect = rectTransform.rect;
		
		return (m_Mode == Mode.Horizontal ? rect.width : rect.height) * m_Quality;
	}

	double GetSPP()
	{
		if (m_AudioClip == null)
			return 0;
		
		float size = GetSize();
		
		if (size < float.Epsilon * 2)
			return 0;
		
		return m_AudioClip.frequency * m_Duration / size;
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		SetAllDirty();
	}
	#endif

	(double min, double max) GetLimits()
	{
		if (m_AudioClip == null)
			return (0, 0);
		
		double spp = GetSPP();
		
		double minTime = m_Time - m_Duration * (1 - m_Ratio);
		double maxTime = m_Time + m_Duration * m_Ratio;
		
		double min = m_AudioClip.frequency * minTime / spp;
		double max = m_AudioClip.frequency * maxTime / spp;
		
		return (min, max);
	}

	protected override Vector4 GetUV2()
	{
		(double min, double max) = GetLimits();
		
		return new Vector4(
			(float)min,
			(float)max
		);
	}

	protected override Vector4 GetUV3()
	{
		return m_Mode == Mode.Horizontal
			? new Vector4(1, 0)
			: new Vector4(0, 1);
	}

	protected override Vector3 GetNormal()
	{
		return Vector3.zero;
	}

	protected override Vector4 GetTangent()
	{
		return Vector4.zero;
	}

	public float GetMax(double _Time)
	{
		return GetValue(_Time).x;
	}

	public float GetMax(int _Sample)
	{
		return GetValue(_Sample).x;
	}

	public float GetAverage(double _Time)
	{
		return GetValue(_Time).y;
	}

	public float GetAverage(int _Sample)
	{
		return GetValue(_Sample).y;
	}

	public float GetRMS(double _Time)
	{
		return GetValue(_Time).z;
	}

	public float GetRMS(int _Sample)
	{
		return GetValue(_Sample).z;
	}

	public Vector3 GetValue(double _Time)
	{
		if (m_AudioClip == null)
			return Vector3.zero;
		
		int sample = (int)(m_AudioClip.frequency * _Time);
		
		return GetValue(sample);
	}

	public Vector3 GetValue(int _Sample)
	{
		if (Sprite == null || Sprite.texture == null)
			return Vector3.zero;
		
		int size = Sprite.texture.width;
		
		double spp = GetSPP();
		
		int sample = (int)(_Sample / spp);
		
		int x = sample % size;
		int y = sample / size;
		
		Color color = Sprite.texture.GetPixel(x, y);
		
		return new Vector3(color.r, color.g, color.b);
	}

	public async void Render()
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		try
		{
			await RenderAsync(m_TokenSource.Token);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		finally
		{
			m_TokenSource?.Dispose();
			m_TokenSource = null;
		}
	}

	public Task RenderAsync(CancellationToken _Token = default)
	{
		m_Time = 0;
		
		return RenderAsync(m_AudioClip, m_Duration, _Token);
	}

	public async Task RenderAsync(AudioClip _AudioClip, double _Duration, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		m_AudioClip = _AudioClip;
		m_Duration  = _Duration;
		
		double spp = GetSPP();
		
		Texture2D texture = await RenderAudioClip(m_AudioClip, spp, _Token);
		
		_Token.ThrowIfCancellationRequested();
		
		Sprite = texture.CreateSprite();
		
		SetAllDirty();
	}

	static async Task<Texture2D> RenderAudioClip(AudioClip _AudioClip, double _SPP, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (_AudioClip == null || _SPP < double.Epsilon * 2)
			return Texture2D.blackTexture;
		
		int count = (int)(_AudioClip.samples / _SPP);
		
		
		int size = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(count)));
		
		Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, false);
		
		texture.wrapMode   = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Point;
		
		for (int x = 0; x < texture.width; x++)
		for (int y = 0; y < texture.height; y++)
		{
			_Token.ThrowIfCancellationRequested();
			
			texture.SetPixel(x, y, Color.black);
		}
		
		int length = (int)(_SPP * _AudioClip.channels);
		
		Block block = new Block(length);
		
		float[] buffer = new float[length];
		
		for (int i = 0; i < count; i++)
		{
			_Token.ThrowIfCancellationRequested();
			
			int sample = (int)(i * _SPP);
			
			_AudioClip.GetData(buffer, sample);
			
			foreach (float value in buffer)
				block.Add(value);
			
			int x = i % size;
			int y = i / size;
			
			texture.SetPixel(x, y, block.Convert());
			
			block.Clear();
			
			if (i % 256 == 0)
				await Task.Yield();
		}
		
		texture.SetPixel(0, 0, Color.black);
		
		texture.SetPixel(texture.width - 1, texture.height - 1, Color.black);
		
		_Token.ThrowIfCancellationRequested();
		
		texture.Apply(false, true);
		
		return texture;
	}
}
