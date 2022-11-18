using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.RetryMenu)]
public class UIRetryMenu : UIMenu
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Message;
	[SerializeField] Button   m_RetryButton;
	[SerializeField] Button   m_CancelButton;

	[SerializeField, Sound] string m_Sound;

	[Inject] Localization       m_Localization;
	[Inject] SoundProcessor     m_SoundProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	Action m_Retry;
	Action m_Cancel;

	protected override void Awake()
	{
		base.Awake();
		
		m_RetryButton.Subscribe(Retry);
		m_CancelButton.Subscribe(Cancel);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_RetryButton.Unsubscribe(Retry);
		m_CancelButton.Unsubscribe(Cancel);
	}

	public void Setup(string _ID, Action _Retry = null, Action _Cancel = null)
	{
		Setup(
			_ID,
			m_Localization.Get("COMMON_ERROR_TITLE"),
			m_Localization.Get("COMMON_ERROR_MESSAGE"),
			_Retry,
			_Cancel
		);
	}

	public void Setup(
		string _ID,
		string _Title,
		string _Message,
		Action _Retry = null,
		Action _Cancel = null)
	{
		m_StatisticProcessor.LogError(_ID);
		
		m_Title.text   = _Title;
		m_Message.text = _Message;
		
		m_Retry  = _Retry;
		m_Cancel = _Cancel;
		
		if (m_Retry == null)
			m_CancelButton.gameObject.SetActive(true);
		else if (m_Cancel == null)
			m_CancelButton.gameObject.SetActive(false);
		else
			m_CancelButton.gameObject.SetActive(true);
	}

	protected override void OnShowStarted()
	{
		m_HapticProcessor.Process(Haptic.Type.Failure);
		m_SoundProcessor.Play(m_Sound);
	}

	void Retry()
	{
		Hide();
		
		Action action = m_Retry;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}

	void Cancel()
	{
		Hide();
		
		Action action = m_Cancel;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}
}
