using UnityEngine;
using Zenject;

[Menu(MenuType.GameMenu)]
public class UIGameMenu : UIMenu
{
	public IASFSampler Sampler => m_Timeline;

	[SerializeField] UISongLabel m_Label;
	[SerializeField] UITimeline  m_Timeline;
	[SerializeField] GameObject  m_Latency;

	[Inject] SignalBus      m_SignalBus;
	[Inject] AudioManager   m_AudioManager;
	[Inject] SongController m_SongController;
	[Inject] MenuProcessor  m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Label.Setup(m_SongID);
		
		ProcessLatency();
	}

	public async void Pause()
	{
		if (!m_SongController.Pause())
			return;
		
		ProcessLatency();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.PauseMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public void Play()
	{
		ProcessLatency();
	}

	public async void Latency()
	{
		Pause();
		
		m_Latency.SetActive(false);
		
		await m_MenuProcessor.Show(MenuType.LatencyMenu);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		ProcessLatency();
		
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(Pause);
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		ProcessLatency();
		
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(Pause);
	}

	protected override bool OnEscape()
	{
		Pause();
		
		return true;
	}

	void ProcessLatency()
	{
		if (m_AudioManager.HasSettings() || m_AudioManager.GetAudioOutputType() != AudioOutputType.Bluetooth)
			m_Latency.SetActive(false);
		else
			m_Latency.SetActive(true);
	}

	async void OnApplicationPause(bool _Pause)
	{
		if (!_Pause || !Shown)
			return;
		
		if (!m_SongController.Pause())
			return;
		
		ProcessLatency();
		
		await m_MenuProcessor.Show(MenuType.PauseMenu, true);
	}

	async void OnApplicationFocus(bool _Focus)
	{
		if (_Focus || !Shown)
			return;
		
		if (!m_SongController.Pause())
			return;
		
		ProcessLatency();
		
		await m_MenuProcessor.Show(MenuType.PauseMenu, true);
	}
}
