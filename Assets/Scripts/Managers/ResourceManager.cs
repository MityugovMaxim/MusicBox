using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class ResourceManager
{
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
		
		request.completed += _ =>
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
		
		operation.completed += _ =>
		{
			if (_Token.IsCancellationRequested)
				return;
			
			completionSource.TrySetResult(true);
		};
		
		return completionSource.Task;
	}
}
