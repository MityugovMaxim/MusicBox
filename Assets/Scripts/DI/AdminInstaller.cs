using System.Reflection;
using UnityEngine;
using Zenject;

public class AdminInstaller : MonoInstaller
{
	[SerializeField] UIAdminElement        m_AdminElement;
	[SerializeField] UISnapshotElement     m_SnapshotElement;
	[SerializeField] UILanguageElement     m_LanguageElement;
	[SerializeField] UILocalizationElement m_LocalizationElement;
	[SerializeField] UIMapElement          m_MapElement;
	[SerializeField] UIColorsElement       m_ColorsElement;

	public override void InstallBindings()
	{
		InstallProcessor<RolesProcessor>();
		
		Container.DeclareSignal<RolesDataUpdateSignal>().OptionalSubscriber();
		
		Container.BindFactory<object, PropertyInfo, RectTransform, UIField, UIField.Factory>().FromFactory<UIFieldFactory>();
		
		Container.BindFactory<IListField, int, RectTransform, UIListEntry, UIListEntry.Factory>().FromFactory<UIListEntryFactory>();
		
		InstallPool<UIAdminElement, UIAdminElement.Pool>(m_AdminElement);
		
		InstallPool<UISnapshotElement, UISnapshotElement.Pool>(m_SnapshotElement);
		
		InstallPool<UILanguageElement, UILanguageElement.Pool>(m_LanguageElement);
		
		InstallPool<UILocalizationElement, UILocalizationElement.Pool>(m_LocalizationElement);
		
		InstallPool<UIMapElement, UIMapElement.Pool>(m_MapElement);
		
		InstallPool<UIColorsElement, UIColorsElement.Pool>(m_ColorsElement);
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