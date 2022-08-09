using UnityEngine;
using Zenject;

public class ResourceFactory<T> : IFactory<string, T> where T : Object
{
	[Inject] DiContainer m_Container;

	public T Create(string _Path)
	{
		T asset = Resources.Load<T>(_Path);
		
		if (asset == null)
			return null;
		
		T instance = m_Container.InstantiatePrefabForComponent<T>(asset);
		
		return instance;
	}
}