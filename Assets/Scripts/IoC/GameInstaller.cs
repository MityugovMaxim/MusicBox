using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField] UIMainMenu   m_MainMenu;
	[SerializeField] UIPauseMenu  m_PauseMenu;
	[SerializeField] UIGameMenu   m_GameMenu;
	[SerializeField] UIResultMenu m_ResultMenu;

	[SerializeField] UIProgressBar m_ProgressBar;
	[SerializeField] UITimer       m_Timer;

	public override void InstallBindings()
	{
		SignalBusInstaller.Install(Container);
		Container.DeclareSignal<HoldHit>();
		Container.DeclareSignal<HoldMiss>();
		Container.DeclareSignal<HoldSuccess>();
		Container.DeclareSignal<HoldFail>();
		
		Container.DeclareSignal<TapSuccess>();
		Container.DeclareSignal<TapFail>();
		
		Container.DeclareSignal<DoubleSuccess>();
		Container.DeclareSignal<DoubleFail>();
		
		ISampleReceiver[] sampleReceivers =
		{
			m_ProgressBar,
			m_Timer,
		};
		Container.Bind<ISampleReceiver[]>().FromInstance(sampleReceivers).AsSingle();
		
		Container.Bind<Haptic>().FromMethod(Haptic.Create);
		
		Container.Bind<UIMainMenu>().FromInstance(m_MainMenu).AsSingle();
		Container.Bind<UIPauseMenu>().FromInstance(m_PauseMenu).AsSingle();
		Container.Bind<UIGameMenu>().FromInstance(m_GameMenu).AsSingle();
		Container.Bind<UIResultMenu>().FromInstance(m_ResultMenu).AsSingle();
		
		Container.BindInterfacesAndSelfTo<HapticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ScoreProcessor>().FromNew().AsSingle();
		
		Container.Bind<LevelProvider>().FromNew().AsSingle();
		
		Container.BindFactory<string, Level, Level.Factory>().FromFactory<PrefabResourceFactory<Level>>();
		
		Container.BindFactory<string, RectTransform, Thumbnail, Thumbnail.Factory>().FromFactory<PrefabResourceFactory<RectTransform, Thumbnail>>();
	}
}