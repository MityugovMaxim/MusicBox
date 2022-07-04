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

	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] SoundProcessor     m_SoundProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	Action m_Action;

	public void Setup(string _ID, string _Place, string _Title, string _Message, Action _Action = null)
	{
		m_StatisticProcessor.LogError(_ID, _Place);
		
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