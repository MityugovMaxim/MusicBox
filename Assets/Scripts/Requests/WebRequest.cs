using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Networking;

public static class WebRequest
{
	public static Task<AudioClip> LoadAudioClipFile(string _Path, CancellationToken _Token = default)
	{
		return LoadAudioClip($"file://{_Path}", AudioType.OGGVORBIS, _Token);
	}

	public static Task<AudioClip> LoadAudioClipFile(string _Path, AudioType _AudioType, CancellationToken _Token = default)
	{
		return LoadAudioClip($"file://{_Path}", _AudioType, _Token);
	}

	public static Task<AudioClip> LoadAudioClip(string _URL, AudioType _AudioType, CancellationToken _Token = default)
	{
		TaskCompletionSource<AudioClip> completionSource = new TaskCompletionSource<AudioClip>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.TrySetCanceled();
			return completionSource.Task;
		}
		
		UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(_URL, _AudioType);
		
		_Token.Register(() => completionSource.TrySetCanceled());
		
		UnityWebRequestAsyncOperation operation = request.SendWebRequest();
		
		operation.completed += _Operation =>
		{
			if (_Token.IsCancellationRequested)
			{
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				Log.Error(typeof(WebRequest), "Load audio clip failed. Connection error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.ProtocolError)
			{
				Log.Error(typeof(WebRequest), "Load audio clip failed. Protocol error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.DataProcessingError)
			{
				Log.Error(typeof(WebRequest), "Load audio clip failed. Data processing error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.Success)
			{
				DownloadHandlerAudioClip handler = request.downloadHandler as DownloadHandlerAudioClip;
				
				if (handler == null)
				{
					Log.Error(typeof(WebRequest), "Load audio clip failed. Download handler is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				AudioClip audioClip = handler.audioClip;
				audioClip.name = Path.GetFileNameWithoutExtension(_URL);
				
				if (audioClip == null)
				{
					Log.Error(typeof(WebRequest), "Load audio clip failed. Audio clip is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				completionSource.TrySetResult(audioClip);
				request.Dispose();
			}
			else
			{
				completionSource.TrySetResult(null);
				request.Dispose();
			}
		};
		
		return completionSource.Task;
	}

	public static Task<Texture2D> LoadTextureFile(string _Path, CancellationToken _Token = default)
	{
		return LoadTexture($"file://{_Path}", _Token);
	}

	public static Task<Texture2D> LoadTexture(string _URL, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_URL))
		{
			Log.Error(typeof(WebRequest), "Load texture failed. URL is null or empty.");
			return null;
		}
		
		TaskCompletionSource<Texture2D> completionSource = new TaskCompletionSource<Texture2D>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.TrySetCanceled();
			return completionSource.Task;
		}
		
		UnityWebRequest request = UnityWebRequestTexture.GetTexture(_URL);
		
		_Token.Register(() => completionSource.TrySetCanceled());
		
		UnityWebRequestAsyncOperation operation = request.SendWebRequest();
		
		operation.completed += _Operation =>
		{
			if (_Token.IsCancellationRequested)
			{
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				Log.Error(typeof(WebRequest), "Load texture failed. Connection error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.ProtocolError)
			{
				Log.Error(typeof(WebRequest), "Load texture failed. Protocol error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.DataProcessingError)
			{
				Log.Error(typeof(WebRequest), "Load texture failed. Data processing error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.Success)
			{
				DownloadHandlerTexture handler = request.downloadHandler as DownloadHandlerTexture;
				
				if (handler == null)
				{
					Log.Error(typeof(WebRequest), "Load texture failed. Download handler is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				Texture2D texture = handler.texture;
				
				if (texture == null)
				{
					Log.Error(typeof(WebRequest), "Load texture failed. Texture is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				completionSource.TrySetResult(texture);
				request.Dispose();
			}
			else
			{
				completionSource.TrySetResult(null);
				request.Dispose();
			}
		};
		
		return completionSource.Task;
	}

	public static Task<byte[]> LoadDataFile(string _Path, CancellationToken _Token = default)
	{
		return LoadData($"file://{_Path}", _Token);
	}

	public static Task<byte[]> LoadData(string _URL, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_URL))
		{
			Log.Error(typeof(WebRequest), "Load text failed. URL is null or empty.");
			return null;
		}
		
		TaskCompletionSource<byte[]> completionSource = new TaskCompletionSource<byte[]>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		UnityWebRequest request = UnityWebRequest.Get(_URL);
		
		UnityWebRequestAsyncOperation operation = request.SendWebRequest();
		
		operation.completed += _Operation =>
		{
			if (_Token.IsCancellationRequested)
			{
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				Log.Error(typeof(WebRequest), "Load data failed. Connection error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.ProtocolError)
			{
				Log.Error(typeof(WebRequest), "Load data failed. Protocol error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.DataProcessingError)
			{
				Log.Error(typeof(WebRequest), "Load data failed. Data processing error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.result == UnityWebRequest.Result.Success)
			{
				if (request.downloadHandler == null)
					completionSource.TrySetException(new UnityException("Corrupted data"));
				else
					completionSource.TrySetResult(request.downloadHandler.data);
				request.Dispose();
			}
			else
			{
				completionSource.TrySetResult(null);
				request.Dispose();
			}
		};
		
		return completionSource.Task;
	}
}