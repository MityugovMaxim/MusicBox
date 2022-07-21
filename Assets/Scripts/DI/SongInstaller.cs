using UnityEngine;
using Zenject;

public class SongInstaller : MonoInstaller
{
	[SerializeField] FXProcessor     m_FXProcessor;
	[SerializeField] UIInputReceiver m_InputReceiver;

	[SerializeField] UITapIndicator    m_TapIndicator;
	[SerializeField] UIDoubleIndicator m_DoubleIndicator;
	[SerializeField] UIHoldIndicator   m_HoldIndicator;

	public override void InstallBindings()
	{
		Container.BindInterfacesAndSelfTo<FXProcessor>().FromInstance(m_FXProcessor).AsSingle();
		
		Container.Bind<UIInputReceiver>().FromInstance(m_InputReceiver).AsSingle();
		
		InstallPool<UITapIndicator, UITapIndicator.Pool>(m_TapIndicator, 6);
		InstallPool<UIDoubleIndicator, UIDoubleIndicator.Pool>(m_DoubleIndicator, 4);
		InstallPool<UIHoldIndicator, UIHoldIndicator.Pool>(m_HoldIndicator, 4);
	}

	void InstallPool<TItem, TPool>(TItem _Prefab, int _Capacity) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(_Capacity)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
	}
}
