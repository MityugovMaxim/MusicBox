using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.RetryMenu)]
public class UIRetryDialog : UIDialog
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Message;
	[SerializeField] Button   m_RetryButton;

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
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_RetryButton.Unsubscribe(Retry);
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
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_HapticProcessor.Process(Haptic.Type.Failure);
		m_SoundProcessor.Play(m_Sound);
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		InvokeCancel();
	}

	void Retry()
	{
		InvokeRetry();
		
		Hide();
	}

	void InvokeRetry()
	{
		Action action = m_Retry;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}

	void InvokeCancel()
	{
		Action action = m_Cancel;
		m_Retry  = null;
		m_Cancel = null;
		action?.Invoke();
	}
}
