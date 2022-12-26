using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Storage;
using UnityEngine.Purchasing;
using Json = System.Collections.Generic.IDictionary<string, object>;

public class JsonProvider : StorageProvider<Json>
{
	protected override bool Encrypt  => false;
	protected override bool Compress => true;

	public Task<bool> UploadAsync(
		string            _Path,
		Json              _Json,
		Encoding          _Encoding,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		if (string.IsNullOrEmpty(_Path))
			return Task.FromResult(false);
		
		Encoding encoding = _Encoding ?? Encoding.UTF8;
		
		return UploadAsync(
			_Path,
			() =>
			{
				string json = MiniJson.JsonEncode(_Json);
				
				return Task.FromResult(encoding.GetBytes(json));
			},
			new MetadataChange()
			{
				ContentEncoding = encoding.EncodingName,
				ContentType     = "plain/text"
			},
			_Progress,
			_Token
		);
	}

	public async Task<Json> DownloadAsync(
		string            _Path,
		Action<float>     _Progress,
		Encoding          _Encoding,
		CancellationToken _Token = default
	)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (string.IsNullOrEmpty(_Path))
			return null;
		
		await DownloadDataAsync(_Path, _Progress, _Token);
		
		_Progress?.Invoke(1);
		
		if (TryGetTask(_Path, out Task<Json> task))
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
						
						Encoding encoding = _Encoding ?? Encoding.UTF8;
						
						string json = encoding.GetString(_Task.Result);
						
						return MiniJson.JsonDecode(json) as Json;
					},
					CancellationToken.None
				);
			
			CacheTask(_Path, task);
			
			return await task;
		}
		catch (TaskCanceledException)
		{
			Log.Info(this, "Load Json canceled. File: {0}.", url);
			return null;
		}
		catch (OperationCanceledException)
		{
			Log.Info(this, "Load Json canceled. File: {0}.", url);
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
