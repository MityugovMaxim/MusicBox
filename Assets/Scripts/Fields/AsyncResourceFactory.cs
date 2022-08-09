using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class AsyncResourceFactory<T> : IAsyncFactory<string, T> where T : Object
{
	[Inject] DiContainer m_Container;

	public Task<T> Create(string _Path)
	{
		TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();
		
		ResourceRequest request = Resources.LoadAsync<T>(_Path);
		
		request.completed += _ =>
		{
			T asset = request.asset as T;
			
			if (asset == null)
			{
				completionSource.TrySetResult(null);
				return;
			}
			
			T instance = m_Container.InstantiatePrefabForComponent<T>(asset);
			
			completionSource.TrySetResult(instance);
		};
		
		return completionSource.Task;
	}
}