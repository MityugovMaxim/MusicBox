using System;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField] Canvas        m_Canvas;
	[SerializeField] UIProgressBar m_ProgressBar;
	[SerializeField] UITimer       m_Timer;

	[SerializeField] ProductInfo m_NoAdsProduct;

	public override void InstallBindings()
	{
		InstallSignals();
		
		Container.Bind<Canvas>().To<Canvas>().FromInstance(m_Canvas);
		
		//InstallSampleReceivers();
		
		InstallProcessors();
		
		InstallFactories();
		
		InstallAudioManager();
		
		Container.Bind<ProductInfo>().FromScriptableObject(m_NoAdsProduct).AsSingle();
	}

	void InstallSampleReceivers()
	{
		ISampleReceiver[] sampleReceivers =
		{
			m_ProgressBar,
			m_Timer,
		};
		Container.Bind<ISampleReceiver[]>().FromInstance(sampleReceivers).AsSingle();
	}

	void InstallFactories()
	{
		Container.BindFactory<string, Action<Level>, ResourceRequest, Level.Factory>().FromFactory<AsyncPrefabResourceFactory<Level>>();
		
		Container.BindFactory<UIMainMenuItem, UIMainMenuItem, UIMainMenuItem.Factory>().FromFactory<PrefabFactory<UIMainMenuItem>>();
		
		Container.BindFactory<UIShopMenuItem, UIShopMenuItem, UIShopMenuItem.Factory>().FromFactory<PrefabFactory<UIShopMenuItem>>();
		
		Container.BindFactory<UIProductMenuItem, UIProductMenuItem, UIProductMenuItem.Factory>().FromFactory<PrefabFactory<UIProductMenuItem>>();
		
		Container.BindFactory<UIMenu, UIMenu, UIMenu.Factory>().FromFactory<PrefabFactory<UIMenu>>();
	}

	void InstallProcessors()
	{
		Container.Bind<Haptic>().FromMethod(Haptic.Create);
		
		#if UNITY_IOS
		Container.Bind(typeof(AdsProcessor), typeof(IInitializable)).To<iOSAdsProcessor>().FromNew().AsSingle();
		#endif
		
		Container.BindInterfacesAndSelfTo<MenuProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<HapticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<SocialProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<PurchaseProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ScoreProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<LevelProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StatisticProcessor>().FromNew().AsSingle();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
		
		Container.DeclareSignal<PurchaseSignal>();
		
		Container.DeclareSignal<LevelStartSignal>();
		Container.DeclareSignal<LevelPlaySignal>();
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

	void InstallAudioManager()
	{
		Container.BindInterfacesAndSelfTo<AudioManager>().FromNew().AsSingle();
		Container.DeclareSignal<AudioPlaySignal>();
		Container.DeclareSignal<AudioPauseSignal>();
		Container.DeclareSignal<AudioNextTrackSignal>();
		Container.DeclareSignal<AudioPreviousTrackSignal>();
		Container.DeclareSignal<AudioSourceChangedSignal>();
	}
}