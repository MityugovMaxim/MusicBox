using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Compression;
using AudioBox.Logging;
using Firebase.Storage;
using UnityEngine;

public abstract class StorageProvider<T>
{
	const string HASH_KEY = "HASH_{0}";

	protected abstract bool Encrypt  { get; }
	protected abstract bool Compress { get; }

	readonly Dictionary<string, Task<T>>                m_CachedTasks       = new Dictionary<string, Task<T>>();
	readonly Dictionary<string, StorageDownloadHandler> m_DownloadProcesses = new Dictionary<string, StorageDownloadHandler>();
	readonly Dictionary<string, StorageUploadHandler>   m_UploadProcesses   = new Dictionary<string, StorageUploadHandler>();

	public bool IsDownloaded(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return false;
		
		string path = GetLocalPath(_Path);
		
		return File.Exists(path);
	}

	public bool IsDownloading(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return false;
		
		return m_DownloadProcesses.ContainsKey(_Path);
	}

	public async Task<bool> CheckUpdateAsync(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return false;
		
		string localHash  = GetLocalHash(_Path);
		string remoteHash = await GetRemoteHash(_Path);
		
		return localHash == null || localHash != remoteHash;
	}

	public void Delete(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		string path = GetLocalPath(_Path);
		
		if (File.Exists(path))
			File.Delete(path);
	}

	protected void CacheTask(string _Path, Task<T> _Task)
	{
		if (string.IsNullOrEmpty(_Path) || _Task == null)
			return;
		
		m_CachedTasks[_Path] = _Task;
	}

	protected void RemoveTask(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		if (m_CachedTasks.ContainsKey(_Path))
			m_CachedTasks.Remove(_Path);
	}

	protected bool TryGetTask(string _Path, out Task<T> _Task)
	{
		if (string.IsNullOrEmpty(_Path))
		{
			_Task = null;
			return false;
		}
		
		return m_CachedTasks.TryGetValue(_Path, out _Task) && _Task != null;
	}

	protected Task<string> GetURL(string _Path)
	{
		return Encrypt ? DecryptFileAsync(_Path) : Task.FromResult($"file://{GetLocalPath(_Path)}");
	}

	static string GetLocalPath(string _Path) => Path.Combine(Application.persistentDataPath, _Path);

	static void CreateDirectory(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		string path = GetLocalPath(_Path);
		
		string directory = Path.GetDirectoryName(path);
		
		if (string.IsNullOrEmpty(directory) || Directory.Exists(directory))
			return;
		
		Directory.CreateDirectory(directory);
	}

	static string GetLocalHash(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return null;
		
		string path = GetLocalPath(_Path);
		
		if (string.IsNullOrEmpty(path))
			return null;
		
		if (!File.Exists(path))
			return null;
		
		string key = string.Format(HASH_KEY, _Path);
		
		return PlayerPrefs.GetString(key);
	}

	static void SetLocalHash(string _Path, string _MD5)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		string key = string.Format(HASH_KEY, _Path);
		
		PlayerPrefs.SetString(key, _MD5);
	}

	static StorageReference GetReference(string _Path) => FirebaseStorage.DefaultInstance.RootReference.Child(_Path);

	static async Task<string> GetRemoteHash(string _Path)
	{
		StorageReference reference = GetReference(_Path);
		
		StorageMetadata metadata = await reference.GetMetadataAsync();
		
		return metadata?.Md5Hash;
	}

	static async Task EncryptFileAsync(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		string path = GetLocalPath(_Path);
		
		if (File.Exists(path))
			await EncryptionAsync(path);
	}

	static async Task<string> DecryptFileAsync(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return null;
		
		string sourcePath = GetLocalPath(_Path);
		
		if (!File.Exists(sourcePath))
			return null;
		
		string fileName = Path.GetFileNameWithoutExtension(_Path);
		
		string targetPath = $"{Application.temporaryCachePath}/{CRC32.Get(fileName)}.abf";
		
		if (!File.Exists(targetPath))
		{
			File.Copy(sourcePath, targetPath);
			
			await EncryptionAsync(targetPath);
		}
		
		return $"file://{targetPath}";
	}

	static async Task EncryptionAsync(string _Path, int _Size = 2048)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		byte[] key =
		{
			0x11,
			0x17,
			0x1D,
			0x1F,
		};
		
		int    count  = 0;
		byte[] buffer = new byte[Mathf.Max(2048, _Size)];
		
		using (FileStream stream = File.OpenRead(_Path))
		{
			count = stream.Read(buffer, 0, buffer.Length);
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] ^= key[i % key.Length];
		}
		
		using (FileStream stream = File.OpenWrite(_Path))
		{
			stream.Write(buffer, 0, count);
		}
	}

	protected async Task<bool> UploadFileAsync(string _Path, Func<Task<byte[]>> _Data, MetadataChange _Metadata, Action<float> _Progress, CancellationToken _Token = default)
	{
		if (m_UploadProcesses.TryGetValue(_Path, out StorageUploadHandler handler) && handler != null)
		{
			handler.AddListener(_Progress);
			return await handler.Task;
		}
		
		StorageReference reference = GetReference(_Path);
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		handler = new StorageUploadHandler(source.Task, _Progress);
		
		m_UploadProcesses[_Path] = new StorageUploadHandler(source.Task, _Progress);
		
		try
		{
			byte[] data = await _Data.Invoke();
			
			if (Compress)
				data = Compression.Compress(data);
			
			await reference.PutBytesAsync(data, _Metadata, handler, _Token);
			source.TrySetResult(true);
			return true;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Upload bytes canceled. Path: {0}.", _Path);
			source.TrySetResult(false);
			return false;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Upload bytes canceled. Path: {0}.", _Path);
			source.TrySetResult(false);
			return false;
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			source.TrySetException(exception);
			throw;
		}
		finally
		{
			if (m_UploadProcesses.ContainsKey(_Path))
				m_UploadProcesses.Remove(_Path);
			
			handler.Dispose();
		}
	}

	protected async Task<bool> DownloadFileAsync(string _Path, Action<float> _Progress, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_Path))
			return false;
		
		if (m_DownloadProcesses.TryGetValue(_Path, out StorageDownloadHandler handler) && handler != null)
		{
			handler.AddListener(_Progress);
			return await handler.Task;
		}
		
		string localHash  = GetLocalHash(_Path);
		string remoteHash = await GetRemoteHash(_Path);
		
		if (localHash != null && remoteHash != null && localHash == remoteHash)
			return true;
		
		StorageReference reference = GetReference(_Path);
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		handler = new StorageDownloadHandler(source.Task, _Progress);
		
		m_DownloadProcesses[_Path] = new StorageDownloadHandler(source.Task, _Progress);
		
		CreateDirectory(_Path);
		
		try
		{
			string path = GetLocalPath(_Path);
			
			await reference.GetFileAsync(path, handler, _Token);
			
			SetLocalHash(_Path, remoteHash);
			
			if (Encrypt)
				await EncryptFileAsync(_Path);
			
			source.TrySetResult(true);
			return true;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Download canceled. Path: {0}", _Path);
			source.TrySetResult(false);
			return false;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Download canceled. Path: {0}", _Path);
			source.TrySetResult(false);
			return false;
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			source.TrySetException(exception);
			throw;
		}
		finally
		{
			if (m_DownloadProcesses.ContainsKey(_Path))
				m_DownloadProcesses.Remove(_Path);
			
			handler.Dispose();
		}
	}
}
