using System;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField] UIMainMenu    m_MainMenu;
	[SerializeField] UIPauseMenu   m_PauseMenu;
	[SerializeField] UIGameMenu    m_GameMenu;
	[SerializeField] UIResultMenu  m_ResultMenu;
	[SerializeField] UILevelMenu   m_LevelMenu;
	[SerializeField] UILoadingMenu m_LoadingMenu;

	[SerializeField] UIProgressBar m_ProgressBar;
	[SerializeField] UITimer       m_Timer;

	public override void InstallBindings()
	{
		InstallSignals();
		
		ISampleReceiver[] sampleReceivers =
		{
			m_ProgressBar,
			m_Timer,
		};
		Container.Bind<ISampleReceiver[]>().FromInstance(sampleReceivers).AsSingle();
		
		Container.Bind<Haptic>().FromMethod(Haptic.Create);
		
		Container.BindInterfacesAndSelfTo<UIMainMenu>().FromInstance(m_MainMenu).AsSingle();
		Container.BindInterfacesAndSelfTo<UIPauseMenu>().FromInstance(m_PauseMenu).AsSingle();
		Container.BindInterfacesAndSelfTo<UIGameMenu>().FromInstance(m_GameMenu).AsSingle();
		Container.BindInterfacesAndSelfTo<UIResultMenu>().FromInstance(m_ResultMenu).AsSingle();
		Container.BindInterfacesAndSelfTo<UILevelMenu>().FromInstance(m_LevelMenu).AsSingle();
		Container.BindInterfacesAndSelfTo<UILoadingMenu>().FromInstance(m_LoadingMenu).AsSingle();
		
		#if UNITY_IOS
		Container.Bind(typeof(AdsProcessor), typeof(IInitializable)).To<iOSAdsProcessor>().FromNew().AsSingle();
		#endif
		
		Container.BindInterfacesAndSelfTo<PurchaseProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<HapticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ScoreProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<LevelProcessor>().FromNew().AsSingle();
		
		Container.BindFactory<string, Action<Level>, ResourceRequest, Level.Factory>().FromFactory<AsyncPrefabResourceFactory<Level>>();
		
		Container.BindFactory<UIMainMenuTrack, UIMainMenuTrack, UIMainMenuTrack.Factory>().FromFactory<PrefabFactory<UIMainMenuTrack>>();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
		
		Container.DeclareSignal<LevelStartSignal>();
		Container.DeclareSignal<LevelRestartSignal>();
		Container.DeclareSignal<LevelExitSignal>();
		Container.DeclareSignal<LevelFinishSignal>();
		
		Container.DeclareSignal<HoldHitSignal>();
		Container.DeclareSignal<HoldMissSignal>();
		Container.DeclareSignal<HoldSuccessSignal>();
		Container.DeclareSignal<HoldFailSignal>();
		
		Container.DeclareSignal<TapSuccessSignal>();
		Container.DeclareSignal<TapFailSignal>();
		
		Container.DeclareSignal<DoubleSuccessSignal>();
		Container.DeclareSignal<DoubleFailSignal>();
	}
}