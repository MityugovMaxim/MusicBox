using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public static class ResourceManager
{
	public static async Task<T> InstantiateAsync<T>(
		string                   _Path,
		PlaceholderFactory<T, T> _Factory,
		UIEntity                 _Container,
		CancellationToken        _Token = default
	) where T : UIEntity
	{
		T asset = await LoadAsync<T>(_Path, _Token);
		
		T instance = _Factory.Create(asset);
		
		instance.RectTransform.SetParent(_Container.RectTransform, false);
		
		return instance;
	}

	public static Task<T> LoadAsync<T>(string _Path, CancellationToken _Token = default) where T : Object
	{
		TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		_Token.Register(() => completionSource.TrySetCanceled());
		
		ResourceRequest request = Resources.LoadAsync<T>(_Path);
		
		request.completed += _Operation =>
		{
			if (_Token.IsCancellationRequested)
				return;
			
			T asset = request.asset as T;
			
			completionSource.TrySetResult(asset);
		};
		
		return completionSource.Task;
	}

	public static Task UnloadAsync(CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		_Token.Register(() => completionSource.TrySetCanceled());
		
		AsyncOperation operation = Resources.UnloadUnusedAssets();
		
		operation.completed += _Operation =>
		{
			if (_Token.IsCancellationRequested)
				return;
			
			completionSource.TrySetResult(true);
		};
		
		return completionSource.Task;
	}

	public static void Unload(Object _Asset)
	{
		Resources.UnloadAsset(_Asset);
	}
}