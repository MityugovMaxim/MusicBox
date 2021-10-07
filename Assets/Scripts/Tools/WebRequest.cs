using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class WebRequest
{
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
			else if (request.isNetworkError)
			{
				Debug.LogErrorFormat("[WebRequest] Load audio clip failed. Network error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.isHttpError)
			{
				Debug.LogErrorFormat("[WebRequest] Load audio clip failed. Http error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.isDone)
			{
				DownloadHandlerAudioClip handler = request.downloadHandler as DownloadHandlerAudioClip;
				
				if (handler == null)
				{
					Debug.LogErrorFormat("[WebRequest] Load audio clip failed. Download handler is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				AudioClip audioClip = handler.audioClip;
				
				if (audioClip == null)
				{
					Debug.LogErrorFormat("[WebRequest] Load audio clip failed. Audio clip is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				completionSource.TrySetResult(audioClip);
				request.Dispose();
			}
		};
		
		return completionSource.Task;
	}

	public static Task<Texture2D> LoadTexture(string _URL, CancellationToken _Token = default)
	{
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
			else if (request.isNetworkError)
			{
				Debug.LogErrorFormat("[WebRequest] Load texture failed. Network error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.isHttpError)
			{
				Debug.LogErrorFormat("[WebRequest] Load texture failed. Http error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.isDone)
			{
				DownloadHandlerTexture handler = request.downloadHandler as DownloadHandlerTexture;
				
				if (handler == null)
				{
					Debug.LogErrorFormat("[WebRequest] Load texture failed. Download handler is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				Texture2D texture = handler.texture;
				
				if (texture == null)
				{
					Debug.LogErrorFormat("[WebRequest] Load texture failed. Texture is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				completionSource.TrySetResult(texture);
				request.Dispose();
			}
		};
		
		return completionSource.Task;
	}

	public static Task<AssetBundle> LoadAssetBundle(string _URL, CancellationToken _Token = default)
	{
		TaskCompletionSource<AssetBundle> completionSource = new TaskCompletionSource<AssetBundle>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.TrySetCanceled();
			return completionSource.Task;
		}
		
		UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(_URL, 0);
		
		_Token.Register(() => completionSource.TrySetCanceled());
		
		UnityWebRequestAsyncOperation operation = request.SendWebRequest();
		
		operation.completed += _Operation =>
		{
			if (_Token.IsCancellationRequested)
			{
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.isNetworkError)
			{
				Debug.LogErrorFormat("[WebRequest] Load asset bundle failed. Network error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.isHttpError)
			{
				Debug.LogErrorFormat("[WebRequest] Load asset bundle failed. Http error. Error: {0}. URL: '{1}'.", request.error, _URL);
				completionSource.TrySetCanceled();
				request.Dispose();
			}
			else if (request.isDone)
			{
				DownloadHandlerAssetBundle handler = request.downloadHandler as DownloadHandlerAssetBundle;
				
				if (handler == null)
				{
					Debug.LogErrorFormat("[WebRequest] Load asset bundle failed. Download handler is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				AssetBundle assetBundle = handler.assetBundle;
				
				if (assetBundle == null)
				{
					Debug.LogErrorFormat("[WebRequest] Load asset bundle failed. Asset bundle is null. URL: '{0}'.", _URL);
					completionSource.TrySetCanceled();
					request.Dispose();
					return;
				}
				
				completionSource.TrySetResult(assetBundle);
				request.Dispose();
			}
		};
		
		return completionSource.Task;
	}

	public static async Task<Sprite> LoadSprite(string _URL, CancellationToken _Token = default)
	{
		Texture2D texture = await LoadTexture(_URL, _Token);
		
		if (texture == null)
		{
			Debug.LogErrorFormat("[WebRequest] Load sprite failed. Texture is null. URL: '{0}'.", _URL);
			return null;
		}
		
		return Sprite.Create(
			texture,
			new Rect(0, 0, texture.width, texture.height),
			new Vector2(0.5f, 0.5f),
			1
		);
	}
}