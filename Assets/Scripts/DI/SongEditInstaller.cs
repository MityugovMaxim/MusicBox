using UnityEngine;
using Zenject;

public class SongEditInstaller : MonoInstaller
{
	[SerializeField] UIPlayer            m_Player;
	[SerializeField] UIBeat              m_Beat;
	[SerializeField] UIAudioWave         m_AudioWave;
	[SerializeField] UITapClipContext    m_TapClipContext;
	[SerializeField] UIDoubleClipContext m_DoubleClipContext;
	[SerializeField] UIHoldClipContext   m_HoldClipContext;
	[SerializeField] UIColorClipContext  m_ColorClipContext;

	[SerializeField] UIHoldKey m_HoldKey;
	[SerializeField] UIBeatKey m_BeatKey;

	[SerializeField] UIBeatHandle         m_BeatHandle;
	[SerializeField] UICreateTapHandle    m_CreateTapHandle;
	[SerializeField] UICreateDoubleHandle m_CreateDoubleHandle;
	[SerializeField] UICreateHoldHandle   m_CreateHoldHandle;
	[SerializeField] UICreateColorHandle  m_CreateColorHandle;
	[SerializeField] UIRecordHandle       m_RecordHandle;

	public override void InstallBindings()
	{
		InstallPool<UITapClipContext, UITapClipContext.Pool>(m_TapClipContext);
		InstallPool<UIDoubleClipContext, UIDoubleClipContext.Pool>(m_DoubleClipContext);
		InstallPool<UIHoldClipContext, UIHoldClipContext.Pool>(m_HoldClipContext);
		InstallPool<UIColorClipContext, UIColorClipContext.Pool>(m_ColorClipContext);
		InstallPool<UIHoldKey, UIHoldKey.Pool>(m_HoldKey);
		InstallPool<UIBeatKey, UIBeatKey.Pool>(m_BeatKey);
		
		Container.Bind<UIPlayer>().FromInstance(m_Player).AsSingle();
		Container.Bind<UIBeat>().FromInstance(m_Beat).AsSingle();
		Container.Bind<UIAudioWave>().FromInstance(m_AudioWave).AsSingle();
		
		Container.Bind<UIBeatHandle>().FromInstance(m_BeatHandle).AsSingle();
		Container.Bind<UICreateTapHandle>().FromInstance(m_CreateTapHandle).AsSingle();
		Container.Bind<UICreateDoubleHandle>().FromInstance(m_CreateDoubleHandle).AsSingle();
		Container.Bind<UICreateHoldHandle>().FromInstance(m_CreateHoldHandle).AsSingle();
		Container.Bind<UICreateColorHandle>().FromInstance(m_CreateColorHandle).AsSingle();
		Container.Bind<UIRecordHandle>().FromInstance(m_RecordHandle).AsSingle();
	}

	void InstallPool<T, TPool>(T _Prefab, int _Capacity = 5) where T : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<T, TPool>()
			.WithInitialSize(_Capacity)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(T).Name}] Pool");
	}
}