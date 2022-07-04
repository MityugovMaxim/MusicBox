using System;
using TMPro;
using UnityEngine;
using Zenject;

[Menu(MenuType.RetryMenu)]
public class UIRetryMenu : UIMenu
{
	[SerializeField] TMP_Text   m_Title;
	[SerializeField] TMP_Text   m_Message;
	[SerializeField] GameObject m_CancelButton;

	[SerializeField, Sound] string m_Sound;

	[Inject] SoundProcessor     m_SoundProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	Action m_Retry;
	Action m_Cancel;

	public void Setup(string _ID, string _Place, string _Title, string _Message, Action _Retry = null, Action _Cancel = null)
	{
		m_StatisticProcessor.LogError(_ID, _Place);
		
		m_Retry  = _Retry;
		m_Cancel = _Cancel;
		
		m_Title.text   = _Title;
		m_Message.text = _Message;
		
		m_CancelButton.SetActive(m_Cancel != null);
	}

	public void Retry()
	{
		Hide();
		
		Action action = m_Retry;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}

	public void Cancel()
	{
		Hide();
		
		Action action = m_Cancel;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}

	protected override void OnShowStarted()
	{
		m_HapticProcessor.Process(Haptic.Type.Failure);
		m_SoundProcessor.Play(m_Sound);
	}
}
