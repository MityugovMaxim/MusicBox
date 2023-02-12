using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ConfirmMenu)]
public class UIConfirmMenu : UIDialog
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Message;
	[SerializeField] Button   m_ConfirmButton;

	[SerializeField, Sound] string m_Sound;

	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	Action m_Confirm;
	Action m_Cancel;

	protected override void Awake()
	{
		base.Awake();
		
		m_ConfirmButton.Subscribe(Confirm);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ConfirmButton.Unsubscribe(Confirm);
	}

	public void Setup(
		string _Title,
		string _Message,
		Action _Confirm = null,
		Action _Cancel  = null
	)
	{
		m_Confirm = _Confirm;
		m_Cancel  = _Cancel;
		
		m_Title.text   = _Title;
		m_Message.text = _Message;
	}

	void Confirm()
	{
		InvokeConfirm();
		
		Hide();
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		m_SoundProcessor.Play(m_Sound);
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		InvokeCancel();
	}

	void InvokeConfirm()
	{
		Action action = m_Confirm;
		m_Confirm = null;
		m_Cancel  = null;
		action?.Invoke();
	}

	void InvokeCancel()
	{
		Action action = m_Cancel;
		m_Confirm = null;
		m_Cancel  = null;
		action?.Invoke();
	}
}
