using UnityEngine;
using Zenject;

public class FeatureInstaller : MonoInstaller
{
	protected void InstallSingleton<T>()
	{
		Container.BindInterfacesAndSelfTo<T>().FromNew().AsSingle();
	}

	protected void InstallComponent<T>() where T : Component
	{
		Container.BindInterfacesAndSelfTo<T>()
			.FromNewComponentOnNewGameObject()
			.WithGameObjectName(typeof(T).Name)
			.AsSingle();
	}

	protected void InstallPool<TItem, TPool>(TItem _Prefab, int _Capacity = 5) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(_Capacity)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
	}
}
