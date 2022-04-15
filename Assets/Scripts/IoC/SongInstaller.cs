using UnityEngine;
using Zenject;

public class SongInstaller : MonoInstaller
{
	[SerializeField] FXProcessor       m_FXProcessor;
	[SerializeField] UIInputReceiver   m_InputReceiver;
	[SerializeField] UITapIndicator    m_TapIndicator;
	[SerializeField] UIDoubleIndicator m_DoubleIndicator;
	[SerializeField] UIHoldIndicator   m_HoldIndicator;
	[SerializeField] UITapFX           m_TapFX;
	[SerializeField] UIDoubleFX        m_DoubleFX;
	[SerializeField] UIHoldFX          m_HoldFX;

	public override void InstallBindings()
	{
		Container.BindInterfacesAndSelfTo<FXProcessor>().FromInstance(m_FXProcessor).AsSingle();
		Container.Bind<UIInputReceiver>().FromInstance(m_InputReceiver).AsSingle();
		
		InstallPool<UITapIndicator, UITapIndicator.Pool>(m_TapIndicator, 6);
		InstallPool<UIDoubleIndicator, UIDoubleIndicator.Pool>(m_DoubleIndicator, 4);
		InstallPool<UIHoldIndicator, UIHoldIndicator.Pool>(m_HoldIndicator, 4);
		InstallPool<UITapFX, UITapFX.Pool>(m_TapFX, 6);
		InstallPool<UIDoubleFX, UIDoubleFX.Pool>(m_DoubleFX, 4);
		InstallPool<UIHoldFX, UIHoldFX.Pool>(m_HoldFX, 4);
	}

	void InstallPool<TItem, TPool>(TItem _Prefab, int _InitialSize) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(_InitialSize)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
	}
}
