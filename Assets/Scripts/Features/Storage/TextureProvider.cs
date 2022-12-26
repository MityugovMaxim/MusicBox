using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Storage;
using UnityEngine;

public class TextureProvider : StorageProvider<Texture2D>
{
	protected override bool Encrypt  => false;
	protected override bool Compress => false;

	public Task<bool> UploadJPGAsync(
		string            _Path,
		Texture2D         _Texture,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		if (string.IsNullOrEmpty(_Path))
			return Task.FromResult(false);
		
		return UploadAsync(
			_Path,
			() => Task.FromResult(_Texture.EncodeToJPG()),
			new MetadataChange()
			{
				ContentEncoding = Encoding.UTF8.EncodingName,
				ContentType     = "image/jpeg"
			},
			_Progress,
			_Token
		);
	}

	public Task<bool> UploadPNGAsync(
		string            _Path,
		Texture2D         _Texture,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		if (string.IsNullOrEmpty(_Path))
			return Task.FromResult(false);
		
		return UploadAsync(
			_Path,
			() => Task.FromResult(_Texture.EncodeToPNG()),
			new MetadataChange()
			{
				ContentEncoding = Encoding.UTF8.EncodingName,
				ContentType     = "image/png"
			},
			_Progress,
			_Token
		);
	}

	public async Task<Texture2D> DownloadAsync(Uri _Uri, CancellationToken _Token = default)
	{
		if (_Uri == null)
			return null;
		
		string url = _Uri.OriginalString;
		
		if (TryGetTask(url, out Task<Texture2D> task))
			return await task;
		
		try
		{
			task = WebRequest.LoadTexture(url, _Token);
			
			CacheTask(url, task);
			
			return await task;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Load Texture canceled. File: {0}.", url);
			return null;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Load Texture canceled. File: {0}.", url);
			return null;
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			throw;
		}
		finally
		{
			RemoveTask(url);
		}
	}

	public async Task<Texture2D> DownloadAsync(string _Path, Action<float> _Progress, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (string.IsNullOrEmpty(_Path))
			return null;
		
		await DownloadFileAsync(_Path, _Progress, _Token);
		
		_Progress?.Invoke(1);
		
		if (TryGetTask(_Path, out Task<Texture2D> task))
			return await task;
		
		string url = await GetURL(_Path);
		
		try
		{
			task = WebRequest.LoadTexture(url, _Token);
			
			CacheTask(_Path, task);
			
			return await task;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Load Texture canceled. File: {0}.", url);
			return null;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Load Texture canceled. File: {0}.", url);
			return null;
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			throw;
		}
		finally
		{
			RemoveTask(_Path);
		}
	}
}
