using System;
using TMPro;
using UnityEngine;
using Zenject;

[Menu(MenuType.ErrorMenu)]
public class UIErrorMenu : UIMenu
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Message;

	[SerializeField, Sound] string m_Sound;

	[Inject] Localization       m_Localization;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] SoundProcessor     m_SoundProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	Action m_Action;

	public void Setup(string _ID, Action _Action = null)
	{
		Setup(
			_ID,
			m_Localization.Get("COMMON_ERROR_TITLE"),
			m_Localization.Get("COMMON_ERROR_MESSAGE"),
			_Action
		);
	}

	public void Setup(string _ID, string _Title, string _Message, Action _Action = null)
	{
		m_StatisticProcessor.LogError(_ID);
		
		m_Title.text   = _Title;
		m_Message.text = _Message;
		m_Action       = _Action;
	}

	public async void Close()
	{
		await m_MenuProcessor.Hide(MenuType.ErrorMenu);
		
		Action action = m_Action;
		m_Action = null;
		action?.Invoke();
	}

	protected override void OnShowStarted()
	{
		m_HapticProcessor.Process(Haptic.Type.Failure);
		m_SoundProcessor.Play(m_Sound);
	}
}
