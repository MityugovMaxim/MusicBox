using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ConfirmMenu)]
public class UIConfirmMenu : UIMenu
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Message;
	[SerializeField] Button   m_ConfirmButton;
	[SerializeField] Button   m_CancelButton;

	[SerializeField, Sound] string m_Sound;

	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	string m_Reason;
	Action m_Confirm;
	Action m_Cancel;

	public void Setup(
		string _Reason,
		string _Title,
		string _Message,
		Action _Confirm = null,
		Action _Cancel  = null
	)
	{
		m_Reason  = _Reason;
		m_Confirm = _Confirm;
		m_Cancel  = _Cancel;
		
		m_Title.text   = _Title;
		m_Message.text = _Message;
		
		m_ConfirmButton.onClick.RemoveAllListeners();
		m_ConfirmButton.onClick.AddListener(Confirm);
		
		m_CancelButton.onClick.RemoveAllListeners();
		m_CancelButton.onClick.AddListener(Cancel);
	}

	void Confirm()
	{
		Hide();
		
		Action action = m_Confirm;
		m_Confirm = null;
		m_Cancel  = null;
		action?.Invoke();
	}

	void Cancel()
	{
		Hide();
		
		Action action = m_Cancel;
		m_Confirm = null;
		m_Cancel  = null;
		action?.Invoke();
	}

	protected override void OnShowStarted()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		m_SoundProcessor.Play(m_Sound);
	}
}