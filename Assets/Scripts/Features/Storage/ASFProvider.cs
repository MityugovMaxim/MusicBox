using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;
using AudioBox.Logging;
using Firebase.Storage;
using UnityEngine.Purchasing;

public class ASFProvider : StorageProvider<ASFFile>
{
	protected override bool Encrypt  => false;
	protected override bool Compress => true;

	public Task<bool> UploadAsync(
		string            _Path,
		ASFFile           _ASF,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		IDictionary<string, object> data = _ASF.Serialize();
		
		if (string.IsNullOrEmpty(_Path))
			return Task.FromResult(false);
		
		return UploadAsync(
			_Path,
			() =>
			{
				string json = MiniJson.JsonEncode(data);
				
				return Task.FromResult(Encoding.UTF8.GetBytes(json));
			},
			new MetadataChange()
			{
				ContentEncoding = Encoding.UTF8.EncodingName,
				ContentType     = "plain/text"
			},
			_Progress,
			_Token
		);
	}

	public async Task<ASFFile> DownloadAsync(
		string            _Path,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (string.IsNullOrEmpty(_Path))
			return null;
		
		await DownloadDataAsync(_Path, _Progress, _Token);
		
		_Progress?.Invoke(1);
		
		if (TryGetTask(_Path, out Task<ASFFile> task))
			return await task;
		
		string url = await GetURL(_Path);
		
		try
		{
			task = WebRequest.LoadData(url, _Token)
				.ContinueWith(
					_Task =>
					{
						if (!_Task.IsCompletedSuccessfully)
							return null;
						
						string json = Encoding.UTF8.GetString(_Task.Result);
						
						IDictionary<string, object> data = MiniJson.JsonDecode(json) as IDictionary<string, object>;
						
						ASFFile asf = new ASFFile(data);
						
						return asf;
					},
					CancellationToken.None
				);
			
			CacheTask(_Path, task);
			
			return await task;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Load ASF canceled. File: {0}.", url);
			return null;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Load ASF canceled. File: {0}.", url);
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
