using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public abstract class RemoteImage : UIEntity
{
	[Flags]
	public enum Options
	{
		Blur  = 1 << 1,
		URL   = 1 << 2,
		Pack  = 1 << 3,
		Alpha = 1 << 4,
	}

	public string Path
	{
		get => m_Path;
		set
		{
			if (m_Path == value)
				return;
			
			Unload();
			
			m_Path = value;
			
			Load();
		}
	}

	public abstract Sprite Sprite { get; protected set; }

	protected abstract MaskableGraphic Graphic { get; }

	[SerializeField] UIGroup m_LoaderGroup;
	[SerializeField] Sprite  m_Default;

	[SerializeField, HideInInspector] string  m_Path;
	[SerializeField, HideInInspector] Options m_Options;
	[SerializeField, HideInInspector] string  m_AtlasID;
	[SerializeField, HideInInspector] int     m_AtlasSize = 2048;
	[SerializeField, HideInInspector] int     m_Width     = 512;
	[SerializeField, HideInInspector] int     m_Height    = 512;

	[Inject] TextureProvider m_TextureProvider;

	Atlas                   m_Atlas;
	CancellationTokenSource m_TokenSource;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Load();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Unload();
	}

	public Task SetSpriteAsync(string _Path)
	{
		m_Path = _Path;
		
		return LoadAsync();
	}

	bool CheckOptions(Options _Options)
	{
		return (m_Options & _Options) == _Options;
	}

	void CancelLoad()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Unload()
	{
		CancelLoad();
		
		if (!Graphic.enabled)
			return;
		
		m_Atlas?.Release(Path);
		
		Graphic.enabled = false;
		Sprite          = null;
		m_Path          = null;
		
		if (m_LoaderGroup != null)
			m_LoaderGroup.Show(true);
	}

	async void Load()
	{
		await LoadAsync();
	}

	async Task LoadAsync()
	{
		CancelLoad();
		
		if (string.IsNullOrEmpty(Path))
		{
			Sprite = m_Default;
			
			bool valid = Sprite != null;
			
			Graphic.enabled = valid;
			
			if (valid)
				await HideLoaderAsync(true);
			return;
		}
		
		CreateAtlas();
		
		if (TryGetSprite(out Sprite sprite))
		{
			Sprite          = sprite;
			Graphic.enabled = true;
			
			await HideLoaderAsync(true);
			
			return;
		}
		
		await ShowLoaderAsync(!Graphic.enabled);
		
		Sprite          = null;
		Graphic.enabled = false;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		Texture2D texture = null;
		
		int frame = Time.renderedFrameCount;
		
		try
		{
			texture = await LoadTextureAsync();
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		if (token.IsCancellationRequested)
			return;
		
		bool instant = frame == Time.renderedFrameCount;
		
		if (texture != null)
		{
			Sprite = CreateSprite(texture);
			
			Graphic.enabled = true;
			
			await HideLoaderAsync(instant);
		}
		else if (m_Default != null)
		{
			Sprite = m_Default;
			
			Graphic.enabled = true;
			
			await HideLoaderAsync(instant);
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	Task ShowLoaderAsync(bool _Instant = false)
	{
		return m_LoaderGroup != null
			? m_LoaderGroup.ShowAsync(_Instant)
			: Task.CompletedTask;
	}

	Task HideLoaderAsync(bool _Instant = false)
	{
		return m_LoaderGroup != null
			? m_LoaderGroup.HideAsync(_Instant)
			: Task.CompletedTask;
	}

	Task<Texture2D> LoadTextureAsync()
	{
		if (string.IsNullOrEmpty(Path))
			return Task.FromResult<Texture2D>(null);
		
		return CheckOptions(Options.URL)
			? m_TextureProvider.DownloadAsync(new Uri(Path), CancellationToken.None)
			: m_TextureProvider.DownloadAsync(Path, null, CancellationToken.None);
	}

	void CreateAtlas()
	{
		if (m_Atlas == null && CheckOptions(Options.Pack))
			m_Atlas = Atlas.Create(m_AtlasID, m_AtlasSize, m_Width, m_Height, CheckOptions(Options.Alpha));
	}

	Sprite CreateSprite(Texture2D _Texture)
	{
		if (CheckOptions(Options.Blur))
			_Texture = _Texture.CreateBlur(0.75f);
		
		return CheckOptions(Options.Pack)
			? m_Atlas.Bake(Path, _Texture)
			: _Texture.CreateSprite();
	}

	bool TryGetSprite(out Sprite _Sprite)
	{
		_Sprite = null;
		
		if (m_Atlas == null)
			return false;
		
		if (!m_Atlas.TryAlloc(Path, out Sprite sprite))
			return false;
		
		_Sprite = sprite;
		
		return true;
	}
}
