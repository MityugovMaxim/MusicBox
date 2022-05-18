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

[Preserve]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class StorageProcessor
{
	static readonly Dictionary<string, Task<Texture2D>> m_TextureTasks   = new Dictionary<string, Task<Texture2D>>();
	static readonly Dictionary<string, Task<AudioClip>> m_AudioClipTasks = new Dictionary<string, Task<AudioClip>>();
	static readonly Dictionary<string, Task<string>>    m_TextTasks      = new Dictionary<string, Task<string>>();

	public Task UploadFile(string _RemotePath, string _LocalPath, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (string.IsNullOrEmpty(_RemotePath))
		{
			completionSource.SetResult(false);
			return completionSource.Task;
		}
		
		if (string.IsNullOrEmpty(_LocalPath))
		{
			completionSource.SetResult(false);
			return completionSource.Task;
		}
		
		if (!File.Exists(_LocalPath))
		{
			completionSource.SetResult(false);
			return completionSource.Task;
		}
		
		StorageReference reference = FirebaseStorage.DefaultInstance.GetReference(_RemotePath);
		
		byte[] bytes = File.ReadAllBytes(_LocalPath);
		
		reference.PutBytesAsync(bytes, cancelToken: _Token)
			.ContinueWith(
				_Task =>
				{
					if (_Task.IsFaulted)
						completionSource.SetException(_Task.Exception ?? new Exception("Unknown exception"));
					else if (_Task.IsCanceled)
						completionSource.SetCanceled();
					else if (_Task.IsCompleted)
						completionSource.SetResult(true);
					else
						completionSource.SetResult(false);
				},
				CancellationToken.None
			);
		
		return completionSource.Task;
	}

	public Task<Texture2D> LoadTextureAsync(Uri _Uri, CancellationToken _Token = default)
	{
		if (_Uri == null)
			return null;
		
		string url = _Uri.ToString();
		
		if (string.IsNullOrEmpty(url))
			return null;
		
		if (m_TextureTasks.TryGetValue(url, out Task<Texture2D> task))
			return task;
		
		TaskCompletionSource<Texture2D> completionSource = new TaskCompletionSource<Texture2D>();
		
		m_TextureTasks[url] = completionSource.Task;
		
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

	public Task<Texture2D> LoadTextureAsync(string _RemotePath, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_TextureTasks.TryGetValue(_RemotePath, out Task<Texture2D> task))
			return task;
		
		TaskCompletionSource<Texture2D> completionSource = new TaskCompletionSource<Texture2D>();
		
		m_TextureTasks[_RemotePath] = completionSource.Task;
		
		LoadAssetAsync(_RemotePath, WebRequest.LoadTextureFile, _Token)
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

	public Task<AudioClip> LoadAudioClipAsync(string _RemotePath, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return Task.FromResult<AudioClip>(null);
		
		if (m_AudioClipTasks.TryGetValue(_RemotePath, out Task<AudioClip> task))
			return task;
		
		TaskCompletionSource<AudioClip> completionSource = new TaskCompletionSource<AudioClip>();
		
		m_AudioClipTasks[_RemotePath] = completionSource.Task;
		
		LoadAssetAsync(_RemotePath, WebRequest.LoadAudioClipFile, _Token)
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

	public Task<string> LoadJson(string _RemotePath, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, false, Encoding.UTF8, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, bool _Force, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, _Force, Encoding.UTF8, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, Encoding _Encoding, CancellationToken _Token = default)
	{
		return LoadJson(_RemotePath, false, _Encoding, _Token);
	}

	public Task<string> LoadJson(string _RemotePath, bool _Force, Encoding _Encoding, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (m_TextTasks.TryGetValue(_RemotePath, out Task<string> task))
			return task;
		
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		m_TextTasks[_RemotePath] = completionSource.Task;
		
		LoadDataAsync(_RemotePath, _Force, _Token)
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

	public Task UploadJson(string _RemotePath, string _Json, Encoding _Encoding, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_RemotePath))
			return null;
		
		if (string.IsNullOrEmpty(_Json))
			return null;
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_RemotePath);
		
		byte[] bytes = _Encoding.GetBytes(_Json);
		
		byte[] encode = Compression.Compress(bytes);
		
		return reference.PutBytesAsync(encode, cancelToken: _Token);
	}

	static async Task<byte[]> LoadDataAsync(string _RemotePath, bool _Force, CancellationToken _Token = default)
	{
		string localPath = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(localPath))
			return null;
		
		if (File.Exists(localPath))
		{
			if (_Force)
				await UpdateFileAsync(_RemotePath, localPath, _Token);
			else
				UpdateFile(_RemotePath, localPath, _Token);
			
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
		
		await LoadFileAsync(_RemotePath, localPath, _Token);
		
		if (!File.Exists(localPath))
			return null;
		
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
		
		return null;
	}

	static async Task<T> LoadAssetAsync<T>(string _RemotePath, Func<string, CancellationToken, Task<T>> _Request, CancellationToken _Token = default) where T : Object
	{
		string localPath = Path.Combine(Application.persistentDataPath, _RemotePath);
		
		if (string.IsNullOrEmpty(localPath))
			return null;
		
		if (File.Exists(localPath))
		{
			UpdateFile(_RemotePath, localPath, _Token);
			
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
		
		await LoadFileAsync(_RemotePath, localPath, _Token);
		
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

	static async void LoadFile(string _RemotePath, string _LocalPath)
	{
		await LoadFileAsync(_RemotePath, _LocalPath);
	}

	static async Task LoadFileAsync(string _RemotePath, string _LocalPath, CancellationToken _Token = default)
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
			await reference.GetFileAsync($"file://{_LocalPath}", null, _Token);
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

	static async void UpdateFile(string _RemotePath, string _LocalPath, CancellationToken _Token = default)
	{
		await UpdateFileAsync(_RemotePath, _LocalPath, _Token);
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
		
		if (metadata == null || PlayerPrefs.GetString(_RemotePath) == metadata.Md5Hash)
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
