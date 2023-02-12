using UnityEngine;
using Zenject;

public class AdminInstaller : MonoInstaller
{
	[SerializeField] UIAdminElement m_AdminElement;
	[SerializeField] UIMapElement   m_MapElement;

	public override void InstallBindings()
	{
		InstallProcessor<RolesProcessor>();
		
		Container.BindFactory<IListField, int, RectTransform, UIListEntry, UIListEntry.Factory>().FromFactory<UIListEntryFactory>();
		
		InstallPool<UIAdminElement, UIAdminElement.Pool>(m_AdminElement);
		
		InstallPool<UIMapElement, UIMapElement.Pool>(m_MapElement);
	}

	void InstallProcessor<T>()
	{
		Container.BindInterfacesAndSelfTo<T>().FromNew().AsSingle();
	}

	void InstallPool<TItem, TPool>(TItem _Prefab) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(0)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
	}
}
