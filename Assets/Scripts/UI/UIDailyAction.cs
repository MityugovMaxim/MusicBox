using UnityEngine;
using Zenject;

public class UIDailyAction : UIEntity
{
	static bool Processing { get; set; }

	[SerializeField] UIButton m_CollectButton;
	[SerializeField] UIGroup  m_LoaderGroup;
	[SerializeField] UIFlare  m_Flare;

	[SerializeField]        Haptic.Type m_Haptic;
	[SerializeField, Sound] string      m_Sound;

	[Inject] DailyManager    m_DailyManager;
	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_CollectButton.Subscribe(ProcessAction);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_CollectButton.Unsubscribe(ProcessAction);
	}

	async void ProcessAction()
	{
		if (Processing)
			return;
		
		m_LoaderGroup.Show();
		
		Processing = true;
		
		bool success = await m_DailyManager.CollectAsync();
		
		Processing = false;
		
		m_LoaderGroup.Hide();
		
		if (success)
		{
			m_Flare.Play();
			m_HapticProcessor.Process(m_Haptic);
			m_SoundProcessor.Play(m_Sound);
			return;
		}
		
		await m_MenuProcessor.RetryAsync("daily", ProcessAction);
	}
}
