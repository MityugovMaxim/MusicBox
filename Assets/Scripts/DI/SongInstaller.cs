using UnityEngine;
using Zenject;

public class SongInstaller : MonoInstaller
{
	[SerializeField] FXProcessor     m_FXProcessor;
	[SerializeField] UIInputReceiver m_InputReceiver;

	[SerializeField] UITapIndicator    m_TapIndicator;
	[SerializeField] UIDoubleIndicator m_DoubleIndicator;
	[SerializeField] UIHoldIndicator   m_HoldIndicator;
	[SerializeField] UIIndicatorFX[]   m_IndicatorFXs;

	public override void InstallBindings()
	{
		Container.BindInterfacesAndSelfTo<FXProcessor>().FromInstance(m_FXProcessor).AsSingle();
		
		Container.Bind<UIInputReceiver>().FromInstance(m_InputReceiver).AsSingle();
		
		InstallPool<UITapIndicator, UITapIndicator.Pool>(m_TapIndicator, 6);
		InstallPool<UIDoubleIndicator, UIDoubleIndicator.Pool>(m_DoubleIndicator, 4);
		InstallPool<UIHoldIndicator, UIHoldIndicator.Pool>(m_HoldIndicator, 4);
		
		foreach (UIIndicatorFX indicatorFX in m_IndicatorFXs)
			InstallPool<UIIndicatorFX, UIIndicatorFX.Pool>(indicatorFX, 4, indicatorFX.Type);
	}

	void InstallPool<TItem, TPool>(TItem _Prefab, int _InitialSize) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(_InitialSize)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
	}

	void InstallPool<TItem, TPool>(TItem _Prefab, int _InitialSize, object _ID) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithId(_ID)
			.WithInitialSize(_InitialSize)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
	}
}
