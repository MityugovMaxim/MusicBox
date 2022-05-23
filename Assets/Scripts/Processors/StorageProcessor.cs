using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

public abstract class StorageProgress : IProgress<DownloadState>
{
	public string Path { get; }

	Action<float> m_Progress;

	float m_Value;

	public StorageProgress(string _Path, Action<float> _Progress)
	{
		Path       = _Path;
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

public class StorageProgress<T> : StorageProgress
{
	public Task<T> Task { get; }

	public StorageProgress(string _Path, Task<T> _Task, Action<float> _Progress) : base(_Path, _Progress)
	{
		Task = _Task;
	}
}

[Preserve]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class StorageProcessor
{
	static readonly Dictionary<string, StorageProgress<Texture2D>> m_TextureTasks   = new Dictionary<string, StorageProgress<Texture2D>>();
	static readonly Dictionary<string, StorageProgress<AudioClip>> m_AudioClipTasks = new Dictionary<string, StorageProgress<AudioClip>>();
	static readonly Dictionary<string, StorageProgress<string>>    m_TextTasks      = new Dictionary<string, StorageProgress<string>>();

	public bool IsLoaded(string _RemotePath)
	{
		string localPath = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		return File.Exists(localPath);
	}

	public bool IsLoading(string _RemotePath)
	{
		if (m_TextureTasks.ContainsKey(_RemotePath))
			return true;
		
		if (m_AudioClipTasks.ContainsKey(_RemotePath))
			return true;
		
		if (m_TextTasks.ContainsKey(_RemotePath))
			return true;
		
		return false;
	}

	public void Subscribe(string _RemotePath, Action<float> _Progress)
	{
		if (m_TextureTasks.ContainsKey(_RemotePath))
		{
			StorageProgress progress = m_TextureTasks[_RemotePath];
			progress?.Subscribe(_Progress);
		}
		else if (m_AudioClipTasks.ContainsKey(_RemotePath))
		{
			StorageProgress progress = m_AudioClipTasks[_RemotePath];
			progress?.Subscribe(_Progress);
		}
		else if (m_TextTasks.ContainsKey(_RemotePath))
		{
			StorageProgress progress = m_TextTasks[_RemotePath];
			progress?.Subscribe(_Progress);
		}
	}

	public void Unsubscribe(string _RemotePath, Action<float> _Progress)
	{
		if (m_TextureTasks.ContainsKey(_RemotePath))
		{
			StorageProgress progress = m_TextureTasks[_RemotePath];
			progress?.Unsubscribe(_Progress);
		}
		else if (m_AudioClipTasks.ContainsKey(_RemotePath))
		{
			StorageProgress progress = m_AudioClipTasks[_RemotePath];
			progress?.Unsubscribe(_Progress);
		}
		else if (m_TextTasks.ContainsKey(_RemotePath))
		{
			StorageProgress progress = m_TextTasks[_RemotePath];
			progress?.Unsubscribe(_Progress);
		}
	}

	public Task<Texture2D> LoadTextureAsync(Uri _Uri, Action<float> _Progress, CancellationToken _Token = default)
	{
		if (_Uri == null)
			return null;
		
		string url = _Uri.ToString();
		
		if (string.IsNullOrEmpty(url))
			return null;
		
		if (m_TextureTasks.TryGetValue(url, out StorageProgress<Texture2D> progress))
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<Texture2D> completionSource = new TaskCompletionSource<Texture2D>();
		
		m_TextureTasks[url] = new StorageProgress<Texture2D>(
			url,
			completionSource.Task,
			_Progress
		);
		
		WebRequest.LoadTexture(url, _Token)
			.ContinueWith(
				_Task =>
				{
					if (m_TextureTasks.ContainsKey(url))
						m_TextureTasks.Remove(url);
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
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_TextureTasks.TryGetValue(_RemotePath, out StorageProgress<Texture2D> progress))
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<Texture2D> completionSource = new TaskCompletionSource<Texture2D>();
		
		progress = new StorageProgress<Texture2D>(
			_RemotePath,
			completionSource.Task,
			_Progress
		);
		
		m_TextureTasks[_RemotePath] = progress;
		
		LoadAssetAsync(_RemotePath, WebRequest.LoadTextureFile, progress, _Token)
			.ContinueWith(
				_Task =>
				{
					if (m_TextureTasks.ContainsKey(_RemotePath))
						m_TextureTasks.Remove(_RemotePath);
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

	public Task<AudioClip> LoadAudioClipAsync(string _RemotePath, Action<float> _Progress, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_AudioClipTasks.TryGetValue(_RemotePath, out StorageProgress<AudioClip> progress))
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<AudioClip> completionSource = new TaskCompletionSource<AudioClip>();
		
		progress = new StorageProgress<AudioClip>(
			_RemotePath,
			completionSource.Task,
			_Progress
		);
		
		m_AudioClipTasks[_RemotePath] = progress;
		
		LoadAssetAsync(_RemotePath, WebRequest.LoadAudioClipFile, progress, _Token)
			.ContinueWith(
				_Task =>
				{
					if (m_AudioClipTasks.ContainsKey(_RemotePath))
						m_AudioClipTasks.Remove(_RemotePath);
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

	public Task<string> LoadJson(string _RemotePath, Action<float> _Progress, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, false, Encoding.UTF8, _Progress, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, bool _Force, Action<float> _Progress, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, _Force, Encoding.UTF8, _Progress, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, Encoding _Encoding, Action<float> _Progress, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, false, _Encoding, _Progress, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, bool _Force, Encoding _Encoding, Action<float> _Progress, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_TextTasks.TryGetValue(_RemotePath, out StorageProgress<string> progress))
		{
			progress.Subscribe(_Progress);
			return progress.Task;
		}
		
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		progress = new StorageProgress<string>(
			_RemotePath,
			completionSource.Task,
			_Progress
		);
		
		m_TextTasks[_RemotePath] = progress;
		
		LoadDataAsync(_RemotePath, _Force, progress, _Token)
			.ContinueWith(
				_Task =>
				{
					if (m_TextTasks.ContainsKey(_RemotePath))
						m_TextTasks.Remove(_RemotePath);
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

	static async Task<byte[]> LoadDataAsync(string _RemotePath, bool _Force, StorageProgress _Progress, CancellationToken _Token = default)
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

	static async Task<T> LoadAssetAsync<T>(string _RemotePath, Func<string, CancellationToken, Task<T>> _Request, StorageProgress _Progress, CancellationToken _Token = default) where T : Object
	{
		string localPath = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(localPath))
			return null;
		
		if (File.Exists(localPath))
		{
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

	static async void LoadFile(string _RemotePath, string _LocalPath, StorageProgress _Progress)
	{
		await LoadFileAsync(_RemotePath, _LocalPath, _Progress);
	}

	static async Task LoadFileAsync(string _RemotePath, string _LocalPath, StorageProgress _Progress, CancellationToken _Token = default)
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
			Debug.LogError("[StorageProcessor] Load file failed. Remove path is null or empty");
			return;
		}
		
		if (string.IsNullOrEmpty(_LocalPath))
		{
			Debug.LogError("[StorageProcessor] Load file failed. Local path is null or empty");
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
