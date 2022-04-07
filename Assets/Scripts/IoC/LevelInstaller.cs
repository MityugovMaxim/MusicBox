using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller
{
	[SerializeField] AudioProcessor        m_AudioProcessor;
	[SerializeField] FXProcessor           m_FXProcessor;
	[SerializeField] UIInputReceiver       m_InputReceiver;
	[SerializeField] UITapIndicator        m_TapIndicator;
	[SerializeField] UIDoubleIndicator     m_DoubleIndicator;
	[SerializeField] UIHoldIndicator       m_HoldIndicator;
	[SerializeField] UIComboIndicator      m_ComboIndicator;
	//[SerializeField] UIScoreIndicator      m_ScoreIndicator;
	[SerializeField] UIMultiplierIndicator m_MultiplierIndicator;
	[SerializeField] UIHealthIndicator     m_HealthIndicator;
	[SerializeField] UITapFX               m_TapFX;
	[SerializeField] UIDoubleFX            m_DoubleFX;
	[SerializeField] UIHoldFX              m_HoldFX;

	public override void InstallBindings()
	{
		Container.BindInterfacesAndSelfTo<AudioProcessor>().FromInstance(m_AudioProcessor).AsSingle();
		Container.BindInterfacesAndSelfTo<FXProcessor>().FromInstance(m_FXProcessor).AsSingle();
		//Container.BindInterfacesTo<UIScoreIndicator>().FromInstance(m_ScoreIndicator).AsSingle();
		Container.BindInterfacesTo<UIComboIndicator>().FromInstance(m_ComboIndicator).AsSingle();
		Container.BindInterfacesTo<UIMultiplierIndicator>().FromInstance(m_MultiplierIndicator).AsSingle();
		Container.BindInterfacesTo<UIHealthIndicator>().FromInstance(m_HealthIndicator).AsSingle();
		Container.Bind<UIInputReceiver>().FromInstance(m_InputReceiver).AsSingle();
		
		InstallPool<UITapIndicator, UITapIndicator.Pool>(m_TapIndicator, 6);
		InstallPool<UIDoubleIndicator, UIDoubleIndicator.Pool>(m_DoubleIndicator, 4);
		InstallPool<UIHoldIndicator, UIHoldIndicator.Pool>(m_HoldIndicator, 4);
		InstallPool<UITapFX, UITapFX.Pool>(m_TapFX, 6);
		InstallPool<UIDoubleFX, UIDoubleFX.Pool>(m_DoubleFX, 4);
		InstallPool<UIHoldFX, UIHoldFX.Pool>(m_HoldFX, 4);
	}

	void InstallPool<T0, T1>(T0 _Prefab, int _InitialSize) where T0 : Object where T1 : IMemoryPool
	{
		Container.BindMemoryPool<T0, T1>()
			.WithInitialSize(_InitialSize)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(T0).Name}] Pool");
	}
}
