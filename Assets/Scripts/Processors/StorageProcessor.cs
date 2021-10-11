using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class StorageProcessor
{
	readonly Dictionary<string, Sprite>    m_SpriteCache    = new Dictionary<string, Sprite>();
	readonly Dictionary<string, AudioClip> m_AudioClipCache = new Dictionary<string, AudioClip>();
	readonly Dictionary<string, TextAsset> m_TextAssetCache = new Dictionary<string, TextAsset>();

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
		
		try
		{
			StorageMetadata metadata = await reference.GetMetadataAsync();
			
			if (PlayerPrefs.GetString(_RemotePath) != metadata.Md5Hash || !File.Exists(path))
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
		
		m_AudioClipCache[_RemotePath] = await WebRequest.LoadAudioClip(url, AudioType.OGGVORBIS, _Token);
		
		return m_AudioClipCache[_RemotePath];
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

	public async Task<AudioClip> LoadLevelPreview(string _LevelID, CancellationToken _Token = default)
	{
		return await LoadAudioClip($"Previews/{_LevelID}.ogg", _Token);
	}

	public async Task<Sprite> LoadLevelThumbnail(string _LevelID, CancellationToken _Token = default)
	{
		return await LoadSprite($"Thumbnails/Levels/{_LevelID}.jpg", _Token);
	}

	public async Task<Sprite> LoadProductThumbnail(string _ProductID, CancellationToken _Token = default)
	{
		return await LoadSprite($"Thumbnails/Products/{_ProductID}.jpg", _Token);
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
}
