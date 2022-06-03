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
		Blur = 1 << 1,
		URL  = 1 << 2,
		Pack = 1 << 3,
	}

	public string Path
	{
		get => m_Path;
		set
		{
			if (m_Path == value)
				return;
			
			OnInvisible();
			
			m_Path = value;
		}
	}

	public abstract Sprite Sprite { get; protected set; }

	protected abstract MaskableGraphic Graphic { get; }

	bool Visible => Graphic.canvas != null && GetWorldRect().Overlaps(Graphic.canvas.pixelRect);

	[SerializeField] UIGroup m_LoaderGroup;
	[SerializeField] Sprite  m_Default;

	[SerializeField, HideInInspector] string  m_Path;
	[SerializeField, HideInInspector] Options m_Options;
	[SerializeField, HideInInspector] string  m_AtlasID;
	[SerializeField, HideInInspector] int     m_AtlasSize = 2048;
	[SerializeField, HideInInspector] int     m_Width     = 512;
	[SerializeField, HideInInspector] int     m_Height    = 512;

	[Inject] StorageProcessor m_StorageProcessor;

	bool                    m_State;
	Atlas                   m_Atlas;
	CancellationTokenSource m_TokenSource;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_State = false;
		
		OnInvisible();
	}

	void LateUpdate()
	{
		bool visible = Visible;
		
		if (visible == m_State)
			return;
		
		if (visible)
			OnVisible();
		else
			OnInvisible();
	}

	public Task SetSpriteAsync(string _Path)
	{
		Path = _Path;
		
		return LoadAsync();
	}

	async void OnVisible()
	{
		m_State = true;
		
		await LoadAsync();
	}

	async void OnInvisible()
	{
		m_State = false;
		
		await UnloadAsync();
	}

	bool CheckOptions(Options _Options)
	{
		return (m_Options & _Options) == _Options;
	}

	Task UnloadAsync()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		Graphic.enabled = false;
		
		m_Atlas?.Release(Path);
		
		return Task.CompletedTask;
	}

	async Task LoadAsync()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		CreateAtlas();
		
		bool visible = Graphic.enabled;
		
		if (TryGetSprite(out Sprite sprite))
		{
			Sprite          = sprite;
			Graphic.enabled = true;
			
			await HideLoaderAsync(visible);
			
			return;
		}
		
		await ShowLoaderAsync(!visible);
		
		Sprite          = null;
		Graphic.enabled = false;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		Texture2D texture = null;
		
		int frame = Time.frameCount;
		
		try { texture = await LoadTextureAsync(); }
		catch (TaskCanceledException) { }
		catch (Exception exception) { Log.Exception(this, exception); }
		
		if (token.IsCancellationRequested)
			return;
		
		bool instant = frame == Time.frameCount;
		
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
			? m_StorageProcessor.LoadTextureAsync(new Uri(Path), null, CancellationToken.None)
			: m_StorageProcessor.LoadTextureAsync(Path, null, CancellationToken.None);
	}

	void CreateAtlas()
	{
		if (m_Atlas == null && CheckOptions(Options.Pack))
			m_Atlas = Atlas.Create(m_AtlasID, m_AtlasSize, m_Width, m_Height);
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