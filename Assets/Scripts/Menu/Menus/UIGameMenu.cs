using UnityEngine;
using Zenject;

[Menu(MenuType.GameMenu)]
public class UIGameMenu : UIMenu
{
	public IASFSampler Sampler => m_Timeline;

	[SerializeField] UISongLabel m_Label;
	[SerializeField] UITimeline  m_Timeline;

	[Inject] SignalBus      m_SignalBus;
	[Inject] SongController m_SongController;
	[Inject] MenuProcessor  m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Label.Setup(m_SongID);
	}

	public async void Pause()
	{
		m_SongController.Pause();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.PauseMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	protected override void OnShowStarted()
	{
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(Pause);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(Pause);
	}

	async void OnApplicationFocus(bool _Focus)
	{
		if (_Focus || !Shown)
			return;
		
		m_SongController.Pause();
		
		await m_MenuProcessor.Show(MenuType.PauseMenu, true);
	}
}