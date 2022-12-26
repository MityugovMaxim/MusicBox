using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Menu(MenuType.CoinsMenu)]
public class UICoinsMenu : UIMenu
{
	public string Reason { get; private set; }

	[SerializeField] TMP_Text    m_Title;
	[SerializeField] TMP_Text    m_Message;
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] Button      m_ConfirmButton;
	[SerializeField] Button      m_CancelButton;

	Action m_Confirm;
	Action m_Cancel;

	protected override void Awake()
	{
		base.Awake();
		
		m_ConfirmButton.Subscribe(Confirm);
		m_CancelButton.Subscribe(Cancel);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ConfirmButton.Unsubscribe(Confirm);
		m_CancelButton.Unsubscribe(Cancel);
	}

	public void Setup(string _ID, string _Title, string _Message, long _Coins, Action _Confirm, Action _Cancel)
	{
		Reason = _ID;
		
		m_Title.text   = _Title;
		m_Message.text = _Message;
		m_Coins.Value  = _Coins;
		
		m_Confirm = _Confirm;
		m_Cancel  = _Cancel;
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
}
