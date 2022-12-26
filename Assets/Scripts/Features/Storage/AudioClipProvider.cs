using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AudioClipProvider : StorageProvider<AudioClip>
{
	protected override bool Encrypt  => true;
	protected override bool Compress => false;

	public Task<bool> UploadAsync(
		string                     _Path,
		AudioClip                  _AudioClip,
		float                      _Quality,
		Dictionary<string, string> _Tags     = null,
		Action<float>              _Progress = null,
		CancellationToken          _Token    = default
	)
	{
		if (string.IsNullOrEmpty(_Path))
			return Task.FromResult(false);
		
		return UploadAsync(
			_Path,
			() => _AudioClip.EncodeToOGG(_Quality, _Tags),
			new MetadataChange()
			{
				ContentEncoding = Encoding.UTF8.EncodingName,
				ContentType     = "audio/ogg"
			},
			_Progress,
			_Token
		);
	}

	public async Task<AudioClip> LoadAsync(string _Path, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_Path))
			return null;
		
		if (!IsDownloaded(_Path))
			await DownloadAsync(_Path, null, _Token);
		
		if (TryGetTask(_Path, out Task<AudioClip> task))
			return await task;
		
		string url = await GetURL(_Path);
		
		try
		{
			task = WebRequest.LoadAudioClip(url, AudioType.OGGVORBIS, _Token);
			
			CacheTask(_Path, task);
			
			return await task;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Load AudioClip canceled. File: {0}.", url);
			return null;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Load AudioClip canceled. File: {0}.", url);
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

	public async Task<AudioClip> DownloadAsync(
		string            _Path,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (string.IsNullOrEmpty(_Path))
			return null;
		
		await DownloadFileAsync(_Path, _Progress, _Token);
		
		_Progress?.Invoke(1);
		
		if (TryGetTask(_Path, out Task<AudioClip> task))
			return await task;
		
		string url = await GetURL(_Path);
		
		try
		{
			task = WebRequest.LoadAudioClip(url, AudioType.OGGVORBIS, _Token);
			
			CacheTask(_Path, task);
			
			return await task;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Load AudioClip canceled. File: {0}.", url);
			return null;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Load AudioClip canceled. File: {0}.", url);
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
