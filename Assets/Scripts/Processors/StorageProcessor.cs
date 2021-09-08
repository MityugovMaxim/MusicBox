using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

public class StorageProcessor : MonoBehaviour, IInitializable, IDisposable
{
	readonly Dictionary<string, Sprite>    m_LevelThumbnails    = new Dictionary<string, Sprite>();
	readonly Dictionary<string, Sprite>    m_LevelBackgrounds   = new Dictionary<string, Sprite>();
	readonly Dictionary<string, Sprite>    m_ProductThumbnails  = new Dictionary<string, Sprite>();
	readonly Dictionary<string, Sprite>    m_ProductBackgrounds = new Dictionary<string, Sprite>();
	readonly Dictionary<string, AudioClip> m_Previews           = new Dictionary<string, AudioClip>();

	FirebaseStorage  m_Storage;
	StorageReference m_ThumbnailsReference;
	StorageReference m_PreviewsReference;
	StorageReference m_SoundsReference;
	StorageReference m_LevelsReference;

	readonly Dictionary<string, Action<Sprite>>    m_LevelActions   = new Dictionary<string, Action<Sprite>>();
	readonly Dictionary<string, Action<Sprite>>    m_ProductActions = new Dictionary<string, Action<Sprite>>();
	readonly Dictionary<string, Action<AudioClip>> m_PreviewActions = new Dictionary<string, Action<AudioClip>>();

	void IInitializable.Initialize()
	{
		m_Storage = FirebaseStorage.DefaultInstance;
		
		StorageReference reference = m_Storage.GetReferenceFromUrl("gs://audiobox-76b0e.appspot.com");
		
		m_ThumbnailsReference = reference.Child("Thumbnails");
		m_PreviewsReference   = reference.Child("Previews");
		m_SoundsReference     = reference.Child("Sounds");
		m_LevelsReference     = reference.Child("Levels");
	}

	void IDisposable.Dispose()
	{
		m_LevelThumbnails.Clear();
		m_LevelBackgrounds.Clear();
		m_Previews.Clear();
	}

	public async void LoadPreview(string _LevelID, Action<AudioClip> _Complete)
	{
		if (m_Previews.ContainsKey(_LevelID) && m_Previews[_LevelID] != null && m_Previews[_LevelID].length > float.Epsilon)
		{
			AudioClip preview = m_Previews[_LevelID];
			_Complete?.Invoke(preview);
			return;
		}
		
		StorageReference reference = m_PreviewsReference.Child($"{_LevelID}.ogg");
		
		if (reference == null)
			return;
		
		if (m_PreviewActions.ContainsKey(_LevelID))
		{
			m_PreviewActions[_LevelID] += _Complete;
			return;
		}
		
		m_PreviewActions[_LevelID] = _Complete;
		
		string directory = Path.Combine(Application.persistentDataPath, "Previews");
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		string path = Path.Combine(directory, $"{_LevelID}.ogg");
		
		string url = "file://" + path;
		
		if (File.Exists(path))
		{
			LoadAudioClip(
				url,
				_Preview =>
				{
					m_Previews[_LevelID] = _Preview;
					
					if (m_PreviewActions.ContainsKey(_LevelID))
					{
						Action<AudioClip> action = m_PreviewActions[_LevelID];
						m_PreviewActions.Remove(_LevelID);
						action?.Invoke(_Preview);
					}
				},
				() =>
				{
					if (m_PreviewActions.ContainsKey(_LevelID))
						m_PreviewActions.Remove(_LevelID);
				}
			);
		}
		
		string key = $"{_LevelID}_preview";
		
		StorageMetadata metadata = await reference.GetMetadataAsync();
		
		if (PlayerPrefs.GetString(key, string.Empty) == metadata.Md5Hash && File.Exists(path))
		{
			m_PreviewActions.Remove(_LevelID);
			return;
		}
		
		Debug.LogFormat("[StorageProcessor] Load preview for level with ID '{0}'.", _LevelID);
		
		await reference.GetFileAsync(url);
		
		PlayerPrefs.SetString(key, metadata.Md5Hash);
		
		LoadAudioClip(
			url,
			_Preview =>
			{
				m_Previews[_LevelID] = _Preview;
				
				if (m_PreviewActions.ContainsKey(_LevelID))
				{
					Action<AudioClip> action = m_PreviewActions[_LevelID];
					m_PreviewActions.Remove(_LevelID);
					action?.Invoke(_Preview);
				}
			},
			() =>
			{
				if (m_PreviewActions.ContainsKey(_LevelID))
					m_PreviewActions.Remove(_LevelID);
			}
		);
	}

	public async void LoadLevelThumbnail(string _LevelID, Action<Sprite> _Complete)
	{
		if (m_LevelThumbnails.ContainsKey(_LevelID) && m_LevelThumbnails[_LevelID] != null)
		{
			Sprite thumbnail = m_LevelThumbnails[_LevelID];
			_Complete?.Invoke(thumbnail);
			return;
		}
		
		StorageReference reference = m_ThumbnailsReference.Child("Levels").Child($"{_LevelID}.jpg");
		
		if (reference == null)
			return;
		
		if (m_LevelActions.ContainsKey(_LevelID))
		{
			m_LevelActions[_LevelID] += _Complete;
			return;
		}
		
		m_LevelActions[_LevelID] = _Complete;
		
		string directory = Path.Combine(Application.persistentDataPath, "LevelThumbnails");
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		string path = Path.Combine(directory, $"{_LevelID}.jpg");
		
		string url = "file://" + path;
		
		if (File.Exists(path))
		{
			LoadSprite(
				url,
				_Thumbnail =>
				{
					m_LevelThumbnails[_LevelID] = _Thumbnail;
					
					if (m_LevelActions.ContainsKey(_LevelID))
					{
						Action<Sprite> action = m_LevelActions[_LevelID];
						m_LevelActions.Remove(_LevelID);
						action?.Invoke(_Thumbnail);
					}
				},
				() =>
				{
					if (m_LevelActions.ContainsKey(_LevelID))
						m_LevelActions.Remove(_LevelID);
				}
			);
		}
		
		string key = $"{_LevelID}_level_thumbnail";
		
		StorageMetadata metadata = await reference.GetMetadataAsync();
		
		if (PlayerPrefs.GetString(key, string.Empty) == metadata.Md5Hash && File.Exists(path))
		{
			m_LevelActions.Remove(_LevelID);
			return;
		}
		
		Debug.LogFormat("[StorageProcessor] Load thumbnail for level with ID '{0}'.", _LevelID);
		
		await reference.GetFileAsync(url);
		
		PlayerPrefs.SetString(key, metadata.Md5Hash);
		
		LoadSprite(
			url,
			_Thumbnail =>
			{
				m_LevelThumbnails[_LevelID] = _Thumbnail;
				
				if (m_LevelActions.ContainsKey(_LevelID))
				{
					Action<Sprite> action = m_LevelActions[_LevelID];
					m_LevelActions.Remove(_LevelID);
					action?.Invoke(_Thumbnail);
				}
			},
			() =>
			{
				if (m_LevelActions.ContainsKey(_LevelID))
					m_LevelActions.Remove(_LevelID);
			}
		);
	}

	public async void LoadProductThumbnail(string _ProductID, Action<Sprite> _Complete)
	{
		if (m_ProductThumbnails.ContainsKey(_ProductID) && m_ProductThumbnails[_ProductID] != null)
		{
			Sprite thumbnail = m_ProductThumbnails[_ProductID];
			_Complete?.Invoke(thumbnail);
			return;
		}
		
		StorageReference reference = m_ThumbnailsReference.Child("Products").Child($"{_ProductID}.jpg");
		
		if (reference == null)
			return;
		
		if (m_ProductActions.ContainsKey(_ProductID))
		{
			m_ProductActions[_ProductID] += _Complete;
			return;
		}
		
		m_ProductActions[_ProductID] = _Complete;
		
		string directory = Path.Combine(Application.persistentDataPath, "ProductThumbnails");
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		string path = Path.Combine(directory, $"{_ProductID}.jpg");
		
		string url = "file://" + path;
		
		if (File.Exists(path))
		{
			LoadSprite(
				url,
				_Thumbnail =>
				{
					m_ProductThumbnails[_ProductID] = _Thumbnail;
					
					if (m_ProductActions.ContainsKey(_ProductID))
					{
						Action<Sprite> action = m_ProductActions[_ProductID];
						m_ProductActions.Remove(_ProductID);
						action?.Invoke(_Thumbnail);
					}
				},
				() =>
				{
					if (m_ProductActions.ContainsKey(_ProductID))
						m_ProductActions.Remove(_ProductID);
				}
			);
		}
		
		string key = $"{_ProductID}_product_thumbnail";
		
		StorageMetadata metadata = await reference.GetMetadataAsync();
		
		if (PlayerPrefs.GetString(key, string.Empty) == metadata.Md5Hash && File.Exists(path))
		{
			m_ProductActions.Remove(_ProductID);
			return;
		}
		
		Debug.LogFormat("[StorageProcessor] Load thumbnail for product with ID '{0}'.", _ProductID);
		
		await reference.GetFileAsync(url);
		
		PlayerPrefs.SetString(key, metadata.Md5Hash);
		
		LoadSprite(
			url,
			_Thumbnail =>
			{
				m_ProductThumbnails[_ProductID] = _Thumbnail;
				
				if (m_ProductActions.ContainsKey(_ProductID))
				{
					Action<Sprite> action = m_ProductActions[_ProductID];
					m_ProductActions.Remove(_ProductID);
					action?.Invoke(_Thumbnail);
				}
			},
			() =>
			{
				if (m_ProductActions.ContainsKey(_ProductID))
					m_ProductActions.Remove(_ProductID);
			}
		);
	}

	public void LoadLevelBackground(string _LevelID, Action<Sprite> _Complete)
	{
		if (m_LevelBackgrounds.ContainsKey(_LevelID) && m_LevelBackgrounds[_LevelID] != null)
		{
			Sprite background = m_LevelBackgrounds[_LevelID];
			_Complete?.Invoke(background);
			return;
		}
		
		LoadLevelThumbnail(
			_LevelID,
			_Thumbnail =>
			{
				if (_Thumbnail == null)
				{
					Debug.LogErrorFormat("[LevelProcessor] Get preview background failed. Preview thumbnail is null for level with ID '{0}'.", _LevelID);
					return;
				}
				
				Sprite background = BlurUtility.Blur(_Thumbnail, 0.5f, 8);
				
				m_LevelBackgrounds[_LevelID] = background;
				
				_Complete?.Invoke(background);
			}
		);
	}

	public void LoadProductBackground(string _ProductID, Action<Sprite> _Complete)
	{
		if (m_ProductBackgrounds.ContainsKey(_ProductID) && m_ProductBackgrounds[_ProductID] != null)
		{
			Sprite background = m_ProductBackgrounds[_ProductID];
			_Complete?.Invoke(background);
			return;
		}
		
		LoadProductThumbnail(
			_ProductID,
			_Thumbnail =>
			{
				if (_Thumbnail == null)
				{
					Debug.LogErrorFormat("[LevelProcessor] Get preview background failed. Preview thumbnail is null for level with ID '{0}'.", _ProductID);
					return;
				}
				
				Sprite background = BlurUtility.Blur(_Thumbnail, 0.5f, 8);
				
				m_ProductBackgrounds[_ProductID] = background;
				
				_Complete?.Invoke(background);
			}
		);
	}

	void LoadSprite(string _URL, Action<Sprite> _Success, Action _Failed)
	{
		StartCoroutine(LoadSpriteRoutine(_URL, _Success, _Failed));
	}

	void LoadAudioClip(string _URL, Action<AudioClip> _Success, Action _Failed)
	{
		StartCoroutine(LoadAudioClipRoutine(_URL, _Success, _Failed));
	}

	static IEnumerator LoadSpriteRoutine(string _URL, Action<Sprite> _Success, Action _Failed)
	{
		using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(_URL, true))
		{
			yield return request.SendWebRequest();
			
			if (request.isHttpError)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load sprite failed. URL: {0}. Http error: {1}.", _URL, request.error);
				_Failed?.Invoke();
				yield break;
			}
			
			if (request.isNetworkError)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load sprite failed. URL: {0}. Network error: {1}.", _URL, request.error);
				_Failed?.Invoke();
				yield break;
			}
			
			if (!request.isDone)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load sprite failed. URL: {0}. Unknown error.", _URL);
				_Failed?.Invoke();
				yield break;
			}
			
			DownloadHandlerTexture handler = request.downloadHandler as DownloadHandlerTexture;
			
			if (handler == null)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load sprite failed. URL: {0}. Audio clip handler is null.", _URL);
				_Failed?.Invoke();
				yield break;
			}
			
			if (handler.texture == null)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load sprite failed. URL: {0}. Audio clip is null.", _URL);
				_Failed?.Invoke();
				yield break;
			}
			
			Sprite sprite = Sprite.Create(
				handler.texture,
				new Rect(0, 0, handler.texture.width, handler.texture.height),
				new Vector2(0.5f, 0.5f),
				1
			);
			
			sprite.name = Path.GetFileNameWithoutExtension(_URL);
			
			_Success?.Invoke(sprite);
		}
	}

	static IEnumerator LoadAudioClipRoutine(string _URL, Action<AudioClip> _Success, Action _Failed)
	{
		using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(_URL, AudioType.OGGVORBIS))
		{
			DownloadHandlerAudioClip handler = request.downloadHandler as DownloadHandlerAudioClip;
			
			if (handler == null)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load audio clip failed. URL: {0}. Audio clip handler is null.", _URL);
				_Failed?.Invoke();
				yield break;
			}
			
			handler.compressed  = true;
			handler.streamAudio = false;
			
			yield return request.SendWebRequest();
			
			if (request.isHttpError)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load audio clip failed. URL: {0}. Http error: {1}.", _URL, request.error);
				_Failed?.Invoke();
				yield break;
			}
			
			if (request.isNetworkError)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load audio clip failed. URL: {0}. Network error: {1}.", _URL, request.error);
				_Failed?.Invoke();
				yield break;
			}
			
			if (!request.isDone)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load audio clip failed. URL: {0}. Unknown error.", _URL);
				_Failed?.Invoke();
				yield break;
			}
			
			if (handler.audioClip == null)
			{
				Debug.LogErrorFormat("[StorageProcessor] Load audio clip failed. URL: {0}. Audio clip is null.", _URL);
				_Failed?.Invoke();
				yield break;
			}
			
			AudioClip audioClip = handler.audioClip;
			
			audioClip.name = Path.GetFileNameWithoutExtension(_URL);
			
			_Success?.Invoke(audioClip);
		}
	}
}