using System.Reflection;
using UnityEngine;
using Zenject;

public class AdminInstaller : MonoInstaller
{
	[SerializeField] UIAdminElement    m_AdminElement;
	[SerializeField] UISnapshotElement m_SnapshotElement;

	public override void InstallBindings()
	{
		InstallProcessor<RolesProcessor>();
		
		Container.DeclareSignal<RolesDataUpdateSignal>().OptionalSubscriber();
		
		Container.BindFactory<object, PropertyInfo, RectTransform, UIField, UIField.Factory>().FromFactory<UIFieldFactory>();
		
		Container.BindFactory<IListField, int, RectTransform, UIListEntry, UIListEntry.Factory>().FromFactory<UIListEntryFactory>();
		
		InstallPool<UIAdminElement, UIAdminElement.Pool>(m_AdminElement, 3);
		
		InstallPool<UISnapshotElement, UISnapshotElement.Pool>(m_SnapshotElement, 10);
	}

	void InstallProcessor<T>()
	{
		Container.BindInterfacesAndSelfTo<T>().FromNew().AsSingle();
	}

	void InstallPool<TItem, TPool>(TItem _Prefab, int _Capacity = 5) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(_Capacity)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
	}
}