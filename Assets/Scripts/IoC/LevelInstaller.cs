using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller
{
	[SerializeField] AudioProcessor    m_AudioProcessor;
	[SerializeField] FXProcessor       m_FXProcessor;
	[SerializeField] ColorProcessor    m_ColorProcessor;
	[SerializeField] UIInputZone       m_InputZone;
	[SerializeField] UIInputReceiver   m_InputReceiver;
	[SerializeField] UITapIndicator    m_TapIndicator;
	[SerializeField] UIDoubleIndicator m_DoubleIndicator;
	[SerializeField] UIHoldIndicator   m_HoldIndicator;
	[SerializeField] UITapFX           m_TapFX;
	[SerializeField] UIDoubleFX        m_DoubleFX;
	[SerializeField] UIHoldFX          m_HoldFX;
	[SerializeField] UIScoreLabel      m_ScoreLabel;

	public override void InstallBindings()
	{
		Container.Bind<Sequencer>().FromComponentOnRoot().AsSingle();
		Container.BindInterfacesAndSelfTo<AudioProcessor>().FromInstance(m_AudioProcessor).AsSingle();
		Container.BindInterfacesAndSelfTo<FXProcessor>().FromInstance(m_FXProcessor).AsSingle();
		Container.BindInterfacesAndSelfTo<ColorProcessor>().FromInstance(m_ColorProcessor).AsSingle();
		Container.BindInterfacesTo<UIScoreLabel>().FromInstance(m_ScoreLabel).AsSingle();
		
		Container.BindInterfacesAndSelfTo<UIInputZone>().FromInstance(m_InputZone).AsSingle();
		Container.Bind<UIInputReceiver>().FromInstance(m_InputReceiver).AsSingle();
		
		Container.BindMemoryPool<UITapIndicator, UITapIndicator.Pool>()
			.WithInitialSize(4)
			.FromComponentInNewPrefab(m_TapIndicator)
			.UnderTransformGroup("[UITapIndicator] Pool");
		
		Container.BindMemoryPool<UIDoubleIndicator, UIDoubleIndicator.Pool>()
			.WithInitialSize(2)
			.FromComponentInNewPrefab(m_DoubleIndicator)
			.UnderTransformGroup("[UIDoubleIndicator] Pool");
		
		Container.BindMemoryPool<UIHoldIndicator, UIHoldIndicator.Pool>()
			.WithInitialSize(2)
			.FromComponentInNewPrefab(m_HoldIndicator)
			.UnderTransformGroup("[UIHoldIndicator] Pool");
		
		Container.BindMemoryPool<UITapFX, UITapFX.Pool>()
			.WithInitialSize(2)
			.FromComponentInNewPrefab(m_TapFX)
			.UnderTransformGroup("[UITapFX] Pool");
		
		Container.BindMemoryPool<UIDoubleFX, UIDoubleFX.Pool>()
			.WithInitialSize(1)
			.FromComponentInNewPrefab(m_DoubleFX)
			.UnderTransformGroup("[UIDoubleFX] Pool");
		
		Container.BindMemoryPool<UIHoldFX, UIHoldFX.Pool>()
			.WithInitialSize(1)
			.FromComponentInNewPrefab(m_HoldFX)
			.UnderTransformGroup("[UIHoldFX] Pool");
	}
}
