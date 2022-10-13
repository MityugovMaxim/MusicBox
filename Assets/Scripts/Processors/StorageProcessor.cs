using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Compression;
using AudioBox.Logging;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

public class StorageDownload : IProgress<DownloadState>
{
	Action<float> m_Progress;

	float m_Value;

	public StorageDownload(Action<float> _Progress)
	{
		m_Progress = _Progress;
		m_Value    = 0;
	}

	public void Subscribe(Action<float> _Action)
	{
		if (_Action == null)
			return;
		
		_Action(m_Value);
		
		m_Progress += _Action;
	}

	public void Unsubscribe(Action<float> _Action)
	{
		if (_Action == null)
			return;
		
		_Action(m_Value);
		
		m_Progress -= _Action;
	}

	void IProgress<DownloadState>.Report(DownloadState _State)
	{
		double state = _State.BytesTransferred;
		double count = _State.TotalByteCount;
		m_Value = (float)(state / count);
		m_Progress?.Invoke(m_Value);
	}
}

public class StorageUpload : IProgress<UploadState>
{
	Action<float> m_Progress;
	float         m_Value;

	public StorageUpload(Action<float> _Progress)
	{
		m_Progress = _Progress;
		m_Value    = 0;
	}

	public void Subscribe(Action<float> _Action)
	{
		if (_Action == null)
			return;
		
		_Action(m_Value);
		
		m_Progress += _Action;
	}

	public void Unsubscribe(Action<float> _Action)
	{
		if (_Action == null)
			return;
		
		_Action(m_Value);
		
		m_Progress -= _Action;
	}

	void IProgress<UploadState>.Report(UploadState _State)
	{
		double state = _State.BytesTransferred;
		double count = _State.TotalByteCount;
		m_Value = (float)(state / count);
		m_Progress?.Invoke(m_Value);
	}
}

public class StorageProgress<T> : StorageDownload
{
	public Task<T> Task { get; }

	public StorageProgress(Task<T> _Task, Action<float> _Progress) : base(_Progress)
	{
		Task = _Task;
	}
}

[Preserve]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class StorageProcessor
{
	static Type Tag => typeof(StorageProcessor);

	static readonly Dictionary<string, StorageProgress<Texture2D>> m_Textures   = new Dictionary<string, StorageProgress<Texture2D>>();
	static readonly Dictionary<string, StorageProgress<AudioClip>> m_AudioClips = new Dictionary<string, StorageProgress<AudioClip>>();
	static readonly Dictionary<string, StorageProgress<string>>    m_Texts      = new Dictionary<string, StorageProgress<string>>();

	public bool IsLoaded(string _RemotePath)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return false;
		
		string path = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		return File.Exists(path) || Directory.Exists(path);
	}

	public bool IsLoading(string _RemotePath)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return false;
		
		if (m_Textures.ContainsKey(_RemotePath))
			return true;
		
		if (m_AudioClips.ContainsKey(_RemotePath))
			return true;
		
		if (m_Texts.ContainsKey(_RemotePath))
			return true;
		
		return false;
	}

	public void Subscribe(string _RemotePath, Action<float> _Progress)
	{
		if (m_Textures.ContainsKey(_RemotePath))
		{
			StorageDownload progress = m_Textures[_RemotePath];
			progress?.Subscribe(_Progress);
		}
		else if (m_AudioClips.ContainsKey(_RemotePath))
		{
			StorageDownload progress = m_AudioClips[_RemotePath];
			progress?.Subscribe(_Progress);
		}
		else if (m_Texts.ContainsKey(_RemotePath))
		{
			StorageDownload progress = m_Texts[_RemotePath];
			progress?.Subscribe(_Progress);
		}
	}

	public void Unsubscribe(string _RemotePath, Action<float> _Progress)
	{
		if (m_Textures.ContainsKey(_RemotePath))
		{
			StorageDownload progress = m_Textures[_RemotePath];
			progress?.Unsubscribe(_Progress);
		}
		else if (m_AudioClips.ContainsKey(_RemotePath))
		{
			StorageDownload progress = m_AudioClips[_RemotePath];
			progress?.Unsubscribe(_Progress);
		}
		else if (m_Texts.ContainsKey(_RemotePath))
		{
			StorageDownload progress = m_Texts[_RemotePath];
			progress?.Unsubscribe(_Progress);
		}
	}

	public Task<Texture2D> LoadTextureAsync(Uri _Uri, Action<float> _Progress, CancellationToken _Token = default)
	{
		string remotePath = _Uri?.ToString() ?? string.Empty;
		
		if (string.IsNullOrEmpty(remotePath))
			return null;
		
		if (m_Textures.TryGetValue(remotePath, out StorageProgress<Texture2D> progress))
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<Texture2D> completionSource = new TaskCompletionSource<Texture2D>();
		
		m_Textures[remotePath] = new StorageProgress<Texture2D>(
			completionSource.Task,
			_Progress
		);
		
		WebRequest.LoadTexture(remotePath, _Token)
			.ContinueWith(
				_Task =>
				{
					if (m_Textures.ContainsKey(remotePath))
						m_Textures.Remove(remotePath);
					if (_Task.IsFaulted)
						completionSource.TrySetException(_Task.Exception ?? new Exception("Unknown exception"));
					else if (_Task.IsCanceled)
						completionSource.TrySetCanceled();
					else
						completionSource.TrySetResult(_Task.Result);
				},
				CancellationToken.None
			);
		
		return completionSource.Task;
	}

	public Task<Texture2D> LoadTextureAsync(string _RemotePath, Action<float> _Progress, CancellationToken _Token = default)
	{
		return LoadAsync(_RemotePath, false, _Progress, m_Textures, WebRequest.LoadTextureFile, _Token);
	}

	public Task<AudioClip> LoadAudioClipAsync(string _RemotePath, CancellationToken _Token = default)
	{
		return LoadAudioClipAsync(_RemotePath, null, _Token);
	}

	public Task<AudioClip> LoadAudioClipAsync(string _RemotePath, Action<float> _Progress, CancellationToken _Token = default)
	{
		return LoadAsync(_RemotePath, false, _Progress, m_AudioClips, WebRequest.LoadAudioClipFile, _Token);
	}

	public Task<AudioClip> LoadMusicAsync(string _RemotePath, Action<float> _Progress, CancellationToken _Token = default)
	{
		return LoadEncryptedAsync(_RemotePath, _Progress,m_AudioClips, WebRequest.LoadAudioClipFile, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, bool _Force, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, _Force, null, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, bool _Force, Action<float> _Progress, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, _Force, Encoding.UTF8, _Progress, _Token);
	}

	public Task UploadJson(string _RemotePath, string _Json, Encoding _Encoding, CancellationToken _Token = default)
	{
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		_Encoding ??= Encoding.UTF8;
		
		byte[] bytes = _Encoding.GetBytes(_Json);
		
		byte[] encode = Compression.Compress(bytes);
		
		return reference.PutBytesAsync(encode, null, null, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, bool _Force, Encoding _Encoding, Action<float> _Progress, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_Texts.TryGetValue(_RemotePath, out StorageProgress<string> progress))
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		progress = new StorageProgress<string>(
			completionSource.Task,
			_Progress
		);
		
		m_Texts[_RemotePath] = progress;
		
		LoadDataAsync(_RemotePath, _Force, progress, _Token)
			.ContinueWith(
				_Task =>
				{
					if (m_Texts.ContainsKey(_RemotePath))
						m_Texts.Remove(_RemotePath);
					if (_Task.IsFaulted)
						completionSource.TrySetException(_Task.Exception ?? new Exception("Unknown exception"));
					else if (_Task.IsCanceled)
						completionSource.TrySetCanceled();
					else if (_Task.IsCompleted && _Task.Result != null)
					{
						byte[] decode = Compression.Decompress(_Task.Result);
						
						Encoding encoding = _Encoding ?? Encoding.UTF8;
						
						string json = encoding.GetString(decode);
						
						completionSource.TrySetResult(json);
					}
					else
					{
						completionSource.TrySetResult(null);
					}
				},
				CancellationToken.None
			);
		
		return completionSource.Task;
	}

	static async Task<byte[]> LoadDataAsync(string _RemotePath, bool _Force, StorageDownload _Progress, CancellationToken _Token = default)
	{
		string localPath = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(localPath))
			return null;
		
		if (File.Exists(localPath))
		{
			if (_Force)
				await UpdateFileAsync(_RemotePath, localPath, _Token);
			else
				UpdateFile(_RemotePath, localPath);
			
			try
			{
				return await WebRequest.LoadDataFile(localPath, _Token);
			}
			catch (TaskCanceledException)
			{
				Debug.LogFormat("[StorageProcessor] Load text canceled. Remote path: {0} Local path: {1}.", _RemotePath, localPath);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		
		try
		{
			await LoadFileAsync(_RemotePath, localPath, _Progress, _Token);
			
			if (!File.Exists(localPath))
				return null;
			
			return await WebRequest.LoadDataFile(localPath, _Token);
		}
		catch (TaskCanceledException)
		{
			Debug.LogFormat("[StorageProcessor] Load data canceled. Remote path: {0} Local path: {1}.", _RemotePath, localPath);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return null;
	}

	static async Task<T> LoadEncryptedAssetAsync<T>(
		string                                   _RemotePath,
		Func<string, CancellationToken, Task<T>> _Request,
		StorageDownload                          _Progress,
		CancellationToken                        _Token = default
	) where T : Object
	{
		string path = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(path))
			return null;
		
		string directory = Path.GetDirectoryName(path);
		
		if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		StorageMetadata metadata = await reference.GetMetadataAsync();
		
		if (metadata == null)
		{
			Log.Error(Tag, "Load encrypted asset failed. File not found at remote path '{0}'.", _RemotePath);
			return null;
		}
		
		string key = $"METADATA_{CRC32.Get(_RemotePath)}";
		
		try
		{
			if (File.Exists(path) && PlayerPrefs.GetString(key) == metadata.Md5Hash)
				return await FileEncryption.Load(path, _Request, _Token);
		}
		catch (Exception)
		{
			Log.Warning(Tag, "Load encrypted asset failed. Trying to download it...");
		}
		
		byte[] data = await reference.GetBytesAsync(1024 * 1024 * 20, _Progress, _Token);
		
		await FileEncryption.Save(path, data, _Token);
		
		PlayerPrefs.SetString(key, metadata.Md5Hash);
		
		return await FileEncryption.Load(path, _Request, _Token);
	}

	static async Task<T> LoadAssetAsync<T>(
		string                                   _RemotePath,
		bool                                     _Update,
		Func<string, CancellationToken, Task<T>> _Request,
		StorageDownload                          _Progress,
		CancellationToken                        _Token = default
	) where T : Object
	{
		string localPath = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(localPath))
			return null;
		
		if (File.Exists(localPath))
		{
			if (_Update)
				await UpdateFileAsync(_RemotePath, localPath, _Token);
			else
				UpdateFile(_RemotePath, localPath);
			
			try
			{
				return await _Request.Invoke(localPath, _Token);
			}
			catch (TaskCanceledException)
			{
				Debug.LogFormat("[StorageProcessor] Load asset canceled. Remote path: {0} Local path: {1}.", _RemotePath, localPath);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		
		await LoadFileAsync(_RemotePath, localPath, _Progress, _Token);
		
		if (!File.Exists(localPath))
			return null;
		
		try
		{
			return await _Request.Invoke(localPath, _Token);
		}
		catch (TaskCanceledException)
		{
			Debug.LogFormat("[StorageProcessor] Load asset canceled. Remote path: {0} Local path: {1}.", _RemotePath, localPath);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return null;
	}

	static Task<T> LoadEncryptedAsync<T>(
		string                                   _RemotePath,
		Action<float>                            _Progress,
		IDictionary<string, StorageProgress<T>>  _Registry,
		Func<string, CancellationToken, Task<T>> _Request,
		CancellationToken                        _Token = default
	) where T : Object
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (_Token.IsCancellationRequested)
			return null;
		
		if (_Registry != null && _Registry.TryGetValue(_RemotePath, out StorageProgress<T> progress) && progress != null)
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();
		
		progress = new StorageProgress<T>(completionSource.Task, _Progress);
		
		if (_Registry != null)
			_Registry[_RemotePath] = progress;
		
		LoadEncryptedAssetAsync(_RemotePath, _Request, progress, _Token)
			.ContinueWith(
				_Task =>
				{
					if (_Registry != null && _Registry.ContainsKey(_RemotePath))
						_Registry.Remove(_RemotePath);
					if (_Task.IsFaulted)
						completionSource.TrySetException(_Task.Exception ?? new Exception("Unknown exception"));
					else if (_Task.IsCanceled)
						completionSource.TrySetCanceled();
					else if (_Task.IsCompleted)
						completionSource.TrySetResult(_Task.Result);
					else
						completionSource.TrySetResult(null);
				},
				CancellationToken.None
			);
		
		return completionSource.Task;
	}

	static Task<T> LoadAsync<T>(
		string                                   _RemotePath,
		bool                                     _Update,
		Action<float>                            _Progress,
		IDictionary<string, StorageProgress<T>>  _Registry,
		Func<string, CancellationToken, Task<T>> _Request,
		CancellationToken                        _Token = default
	) where T : Object
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return Task.FromResult<T>(null);
		
		if (_Token.IsCancellationRequested)
			return Task.FromCanceled<T>(_Token);
		
		if (_Registry != null && _Registry.TryGetValue(_RemotePath, out StorageProgress<T> progress) && progress != null)
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();
		
		progress = new StorageProgress<T>(
			completionSource.Task,
			_Progress
		);
		
		if (_Registry != null)
			_Registry[_RemotePath] = progress;
		
		LoadAssetAsync(_RemotePath, _Update, _Request, progress, _Token)
			.ContinueWith(
				_Task =>
				{
					if (_Registry != null && _Registry.ContainsKey(_RemotePath))
						_Registry.Remove(_RemotePath);
					if (_Task.IsFaulted)
						completionSource.TrySetException(_Task.Exception ?? new Exception("Unknown exception"));
					else if (_Task.IsCanceled)
						completionSource.TrySetCanceled();
					else if (_Task.IsCompleted)
						completionSource.TrySetResult(_Task.Result);
					else
						completionSource.TrySetResult(null);
				},
				CancellationToken.None
			);
		
		return completionSource.Task;
	}

	static async Task LoadFileAsync(string _RemotePath, string _LocalPath, StorageDownload _Progress, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
		{
			Debug.LogError("[StorageProcessor] Load file failed. Remove path is null or empty");
			return;
		}
		
		if (string.IsNullOrEmpty(_LocalPath))
		{
			Debug.LogError("[StorageProcessor] Load file failed. Local path is null or empty");
			return;
		}
		
		string directory = Path.GetDirectoryName(_LocalPath);
		
		if (string.IsNullOrEmpty(directory))
			return;
		
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		try
		{
			await reference.GetFileAsync($"file://{_LocalPath}", _Progress, _Token);
		}
		catch (TaskCanceledException)
		{
			Debug.LogFormat("[StorageProcessor] Load file canceled. Remote path: {0} Local path: {1}.", _RemotePath, _LocalPath);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogErrorFormat("[StorageProcessor] Load file failed. Remote path: {0} Local path: {1}.", _RemotePath, _LocalPath);
		}
	}

	static async void UpdateFile(string _RemotePath, string _LocalPath)
	{
		await UpdateFileAsync(_RemotePath, _LocalPath);
	}

	static async Task UpdateFileAsync(string _RemotePath, string _LocalPath, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
		{
			Debug.LogError("[StorageProcessor] Update file failed. Remove path is null or empty");
			return;
		}
		
		if (string.IsNullOrEmpty(_LocalPath))
		{
			Debug.LogError("[StorageProcessor] Update file failed. Local path is null or empty");
			return;
		}
		
		if (!File.Exists(_LocalPath))
		{
			Debug.LogErrorFormat("[StorageProcessor] Update file failed. File not found. Remote path: {0} Local path: {1}.", _RemotePath, _LocalPath);
			return;
		}
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		StorageMetadata metadata = await reference.GetMetadataAsync();
		
		if (PlayerPrefs.GetString(_RemotePath) == metadata.Md5Hash)
			return;
		
		Debug.LogFormat("[StorageProcessor] Updating file '{0}'...", _RemotePath);
		
		try
		{
			await reference.GetFileAsync($"file://{_LocalPath}", null, _Token);
			
			PlayerPrefs.SetString(_RemotePath, metadata.Md5Hash);
		}
		catch (TaskCanceledException)
		{
			Debug.LogFormat("[StorageProcessor] Update file canceled. Remote path: {0} Local path: {1}.", _RemotePath, _LocalPath);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
