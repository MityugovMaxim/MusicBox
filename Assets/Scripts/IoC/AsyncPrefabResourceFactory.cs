using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AsyncPrefabResourceFactory<T> : IFactory<string, Action<T>, ResourceRequest>
{
	[Inject] DiContainer m_Container;

	public ResourceRequest Create(string _Path, Action<T> _Callback)
	{
		ResourceRequest request = Resources.LoadAsync(_Path);
		
		request.completed += _Operation =>
		{
			T asset = m_Container.InstantiatePrefabForComponent<T>(request.asset);
			
			_Callback?.Invoke(asset);
		};
		
		return request;
	}
}