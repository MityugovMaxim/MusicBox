using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;

[Preserve]
public class StorageProcessor
{
	readonly Dictionary<string, string>    m_TextCache      = new Dictionary<string, string>();
	readonly Dictionary<string, Sprite>    m_SpriteCache    = new Dictionary<string, Sprite>();
	readonly Dictionary<string, AudioClip> m_AudioClipCache = new Dictionary<string, AudioClip>();

	public async Task<string> LoadText(string _RemotePath, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return null;
		
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_TextCache.ContainsKey(_RemotePath) && m_TextCache[_RemotePath] != null)
			return m_TextCache[_RemotePath];
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		if (reference == null)
			return null;
		
		string path = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(path))
			return null;
		
		string directory = Path.GetDirectoryName(path);
		
		if (string.IsNullOrEmpty(directory))
			return null;
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		string url = $"file://{path}";
		
		try
		{
			StorageMetadata metadata = await reference.GetMetadataAsync();
			
			if (PlayerPrefs.GetString(_RemotePath) != metadata.Md5Hash || !File.Exists(path))
			{
				Debug.LogFormat("[StorageProcessor] Load text '{0}'", _RemotePath);
				
				await reference.GetFileAsync(url, null, _Token);
				
				PlayerPrefs.SetString(_RemotePath, metadata.Md5Hash);
			}
		}
		catch
		{
			Debug.LogWarningFormat("[StorageProcessor] Load text '{0}' failed. Try to load it from cache.", _RemotePath);
		}
		
		m_TextCache[_RemotePath] = await WebRequest.LoadText(url, _Token);
		
		return m_TextCache[_RemotePath];
	}

	public async Task<Sprite> LoadSprite(Uri _Uri, CancellationToken _Token = default)
	{
		if (_Uri == null)
			return null;
		
		string url = _Uri.AbsolutePath;
		
		if (string.IsNullOrEmpty(url))
			return null;
		
		Sprite sprite = await WebRequest.LoadSprite(url, _Token);
		
		if (sprite == null)
			return null;
		
		m_SpriteCache[url] = sprite;
		
		return sprite;
	}

	public async Task<Sprite> LoadSprite(string _RemotePath, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return null;
		
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_SpriteCache.ContainsKey(_RemotePath) && m_SpriteCache[_RemotePath] != null)
			return m_SpriteCache[_RemotePath];
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		if (reference == null)
			return null;
		
		string path = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(path))
			return null;
		
		string directory = Path.GetDirectoryName(path);
		
		if (string.IsNullOrEmpty(directory))
			return null;
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		string url = $"file://{path}";
		
		if (File.Exists(path))
		{
			ReloadCache(_RemotePath, m_SpriteCache);
			
			m_SpriteCache[_RemotePath] = await WebRequest.LoadSprite(url, _Token);
			
			return m_SpriteCache[_RemotePath];
		}
		
		try
		{
			StorageMetadata metadata = await reference.GetMetadataAsync();
			
			if (PlayerPrefs.GetString(_RemotePath) != metadata.Md5Hash)
			{
				Debug.LogFormat("[StorageProcessor] Load sprite '{0}'", _RemotePath);
				
				await reference.GetFileAsync(url, null, _Token);
				
				PlayerPrefs.SetString(_RemotePath, metadata.Md5Hash);
			}
		}
		catch
		{
			Debug.LogWarningFormat("[StorageProcessor] Load sprite '{0}' failed. Try to load it from cache.", _RemotePath);
		}
		
		m_SpriteCache[_RemotePath] = await WebRequest.LoadSprite(url, _Token);
		
		return m_SpriteCache[_RemotePath];
	}

	public async Task<AudioClip> LoadAudioClip(string _RemotePath, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return null;
		
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_AudioClipCache.ContainsKey(_RemotePath) && m_AudioClipCache[_RemotePath] != null)
			return m_AudioClipCache[_RemotePath];
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		if (reference == null)
			return null;
		
		string path = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(path))
			return null;
		
		string directory = Path.GetDirectoryName(path);
		
		if (string.IsNullOrEmpty(directory))
			return null;
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		string url = $"file://{path}";
		
		if (File.Exists(path))
		{
			ReloadCache(_RemotePath, m_AudioClipCache);
			
			m_AudioClipCache[_RemotePath] = await WebRequest.LoadAudioClip(url, AudioType.OGGVORBIS, _Token);
			
			return m_AudioClipCache[_RemotePath];
		}
		
		try
		{
			StorageMetadata metadata = await reference.GetMetadataAsync();
			
			if (PlayerPrefs.GetString(_RemotePath) != metadata.Md5Hash || !File.Exists(path))
			{
				Debug.LogFormat("[StorageProcessor] Load audio clip '{0}'", _RemotePath);
				
				await reference.GetFileAsync(url, null, _Token);
				
				PlayerPrefs.SetString(_RemotePath, metadata.Md5Hash);
			}
		}
		catch
		{
			Debug.LogWarningFormat("[StorageProcessor] Load audio clip '{0}' failed. Try to load it from cache.", _RemotePath);
		}
		
		try
		{
			m_AudioClipCache[_RemotePath] = await WebRequest.LoadAudioClip(url, AudioType.OGGVORBIS, _Token);
			
			return m_AudioClipCache[_RemotePath];
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[StorageProcessor] Load audio clip '{0}' failed. Error: {1}.", _RemotePath, exception.Message);
		}
		
		return null;
	}

	public async Task<AssetBundle> LoadAssetBundle(string _RemotePath, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return null;
		
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		if (reference == null)
			return null;
		
		string path = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(path))
			return null;
		
		string directory = Path.GetDirectoryName(path);
		
		if (string.IsNullOrEmpty(directory))
			return null;
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		string url = $"file://{path}";
		
		try
		{
			StorageMetadata metadata = await reference.GetMetadataAsync();
			
			if (PlayerPrefs.GetString(_RemotePath) != metadata.Md5Hash || !File.Exists(path))
			{
				Debug.LogFormat("[StorageProcessor] Load asset bundle '{0}'", _RemotePath);
				
				await reference.GetFileAsync(url, null, _Token);
				
				PlayerPrefs.SetString(_RemotePath, metadata.Md5Hash);
			}
		}
		catch
		{
			Debug.LogWarningFormat("[StorageProcessor] Load asset bundle '{0}' failed. Try to load it from cache.", _RemotePath);
		}
		
		return await WebRequest.LoadAssetBundle(url, _Token);
	}

	public async Task<Sprite> LoadLevelThumbnail(string _LevelID, CancellationToken _Token = default)
	{
		return await LoadSprite($"Thumbnails/Levels/{_LevelID}.jpg", _Token);
	}

	public async Task<Sprite> LoadBannerThumbnail(string _BannerID, CancellationToken _Token = default)
	{
		return await LoadSprite($"Thumbnails/Banners/{_BannerID}.jpg", _Token);
	}

	public async Task<Sprite> LoadOfferThumbnail(string _OfferID, CancellationToken _Token = default)
	{
		return await LoadSprite($"Thumbnails/Offers/{_OfferID}.jpg", _Token);
	}

	public async Task<Sprite> LoadNewsThumbnail(string _NewsID, CancellationToken _Token = default)
	{
		return await LoadSprite($"Thumbnails/News/{_NewsID}.jpg", _Token);
	}

	public async Task<Sprite> LoadProductThumbnail(string _ProductID, CancellationToken _Token = default)
	{
		return await LoadSprite($"Thumbnails/Products/{_ProductID}.jpg", _Token);
	}

	public async Task<Dictionary<string, string>> LoadLocalization(string _Language, CancellationToken _Token = default)
	{
		Dictionary<string, string> localization = new Dictionary<string, string>();
		
		string text;
		
		try
		{
			text = await LoadText($"Localization/{_Language}.json", _Token);
		}
		catch (Exception)
		{
			Debug.LogWarningFormat("[StorageProcessor] Load localization failed. Language: {0}.", _Language);
			text = await LoadText($"Localization/{SystemLanguage.English.GetCode()}.json", _Token);
		}
		
		Dictionary<string, object> data = MiniJson.JsonDecode(text) as Dictionary<string, object>;
		
		if (data == null)
			return null;
		
		foreach (var entry in data)
			localization[entry.Key] = entry.Value.ToString();
		
		return localization;
	}

	public async Task<Sprite> LoadLevelBackground(string _LevelID, CancellationToken _Token = default)
	{
		string path = $"Backgrounds/Levels/{_LevelID}.jpg";
		
		if (m_SpriteCache.ContainsKey(path) && m_SpriteCache[path] != null)
			return m_SpriteCache[path];
		
		Sprite thumbnail = await LoadLevelThumbnail(_LevelID, _Token);
		
		if (thumbnail == null)
			return null;
		
		m_SpriteCache[path] = BlurUtility.Blur(thumbnail, 0.5f, 8);
		
		return m_SpriteCache[path];
	}

	public async Task<Sprite> LoadProductBackground(string _ProductID, CancellationToken _Token = default)
	{
		string path = $"Backgrounds/Products/{_ProductID}.jpg";
		
		if (m_SpriteCache.ContainsKey(path) && m_SpriteCache[path] != null)
			return m_SpriteCache[path];
		
		Sprite thumbnail = await LoadProductThumbnail(_ProductID, _Token);
		
		if (thumbnail == null)
			return null;
		
		m_SpriteCache[path] = BlurUtility.Blur(thumbnail, 0.5f, 8);
		
		return m_SpriteCache[path];
	}

	public async Task<Track[]> LoadTracks(string _LevelID, CancellationToken _Token = default)
	{
		#if UNITY_EDITOR
		Track[] tracks = Directory.GetFiles($"Assets/Levels/{_LevelID}/Tracks/", "*.asset")
			.Select(UnityEditor.AssetDatabase.LoadAssetAtPath<Track>)
			.ToArray();
		
		await Task.Delay(500, _Token);
		
		return tracks;
		#else
		AssetBundle assetBundle = await LoadAssetBundle($"Levels/level.{_LevelID}.unity3d", _Token);
		
		Track[] tracks = assetBundle.LoadAllAssets<Track>();
		
		assetBundle.Unload(false);
		
		return tracks;
		#endif
	}

	static async void ReloadCache<T>(string _Path, IDictionary<string, T> _Cache)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_Path);
		
		if (reference == null)
			return;
		
		string path = Path.Combine(Application.persistentDataPath, _Path);
		
		string url = $"file://{path}";
		
		try
		{
			StorageMetadata metadata = await reference.GetMetadataAsync();
			
			if (PlayerPrefs.GetString(_Path) == metadata.Md5Hash && File.Exists(path))
				return;
			
			await reference.GetFileAsync(url);
			
			PlayerPrefs.SetString(_Path, metadata.Md5Hash);
			
			_Cache?.Remove(_Path);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
