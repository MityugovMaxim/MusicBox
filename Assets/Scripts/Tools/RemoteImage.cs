using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public abstract class RemoteImage<T> : UIEntity where T : MaskableGraphic
{
	public string Path
	{
		get => m_Path;
		set
		{
			if (m_Path == value)
				return;
			
			if (Loaded)
				m_Atlas?.Release(m_Path);
			
			m_Path = value;
			
			Load();
		}
	}

	protected T Graphic => m_Graphic;

	protected abstract Sprite Sprite { get; set; }

	bool Loaded { get; set; }

	[SerializeField, HideInInspector] string m_Path;

	[SerializeField] T       m_Graphic;
	[SerializeField] int     m_Size;
	[SerializeField] Sprite  m_Default;
	[SerializeField] UIGroup m_LoaderGroup;
	[SerializeField] bool    m_Blur;
	[SerializeField] bool    m_URL;
	[SerializeField] int     m_AtlasSize = 2048;

	[Inject] StorageProcessor m_StorageProcessor;

	Atlas m_Atlas;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		Graphic.onCullStateChanged.AddListener(OnCull);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		Graphic.onCullStateChanged.RemoveListener(OnCull);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		OnCull(false);
		
		m_Graphic.enabled = Loaded;
		
		if (m_LoaderGroup != null && !Loaded)
			m_LoaderGroup.Show(true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		OnCull(true);
	}

	public async Task Load(string _Path)
	{
		Unload();
		
		m_Path = _Path;
		
		await LoadAsync();
	}

	public async void Reload()
	{
		Unload();
		await LoadAsync();
	}

	void OnCull(bool _Value)
	{
		if (_Value)
			Unload();
		else if (!Loaded)
			Load();
	}

	void Unload()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		Loaded = false;
		
		m_Atlas?.Release(Path);
	}

	async void Load()
	{
		await LoadAsync();
	}

	async Task LoadAsync()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		if (m_Atlas == null && !m_URL)
			m_Atlas = Atlas.Create(m_AtlasSize, new Vector2Int(m_Size, m_Size));
		
		if (m_Atlas != null && m_Atlas.TryAlloc(Path, out Sprite sprite))
		{
			Sprite            = sprite;
			m_Graphic.enabled = true;
			Loaded            = true;
			
			if (m_LoaderGroup != null)
				m_LoaderGroup.Hide();
			
			return;
		}
		
		if (m_LoaderGroup != null)
			await m_LoaderGroup.ShowAsync(m_Graphic.enabled);
		
		Sprite            = null;
		m_Graphic.enabled = false;
		Loaded            = false;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		Texture2D texture = null;
		
		try
		{
			if (!string.IsNullOrEmpty(Path))
			{
				texture = m_URL
					? await m_StorageProcessor.LoadTextureAsync(new Uri(Path), CancellationToken.None)
					: await m_StorageProcessor.LoadTextureAsync(Path, CancellationToken.None);
			}
		}
		catch (TaskCanceledException)
		{
			Debug.LogFormat(this, "[WebImage] Load sprite cancelled. Path: {0}.", Path);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		if (token.IsCancellationRequested)
			return;
		
		if (texture != null)
		{
			texture = m_Blur ? texture.CreateBlur() : texture;
			
			Sprite = m_Atlas != null ? m_Atlas.Bake(Path, texture) : texture.CreateSprite();
			
			m_Graphic.enabled = true;
			Loaded            = true;
			
			if (m_LoaderGroup != null)
				m_LoaderGroup.Hide();
		}
		else if (m_Default != null)
		{
			Sprite = m_Default;
			
			m_Graphic.enabled = true;
			Loaded            = false;
			
			if (m_LoaderGroup != null)
				m_LoaderGroup.Hide();
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}