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

	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	string m_Reason;
	Action m_Retry;
	Action m_Cancel;

	[Inject]
	public void Construct(
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup(string _Reason, string _Title, string _Message, Action _Retry = null, Action _Cancel = null)
	{
		m_Reason       = _Reason;
		m_Title.text   = _Title;
		m_Message.text = _Message;
		m_Retry        = _Retry;
		m_Cancel       = _Cancel;
		
		m_CancelButton.SetActive(m_Cancel != null);
		
		m_StatisticProcessor.LogRetryMenuShow(m_Reason);
	}

	public void Retry()
	{
		m_StatisticProcessor.LogRetryMenuRetryClick(m_Reason);
		
		Hide();
		
		Action action = m_Retry;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}

	public void Cancel()
	{
		m_StatisticProcessor.LogRetryMenuCancelClick(m_Reason);
		
		Hide();
		
		Action action = m_Cancel;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}
}
