using System;
using TMPro;
using UnityEngine;
using Zenject;

[Menu(MenuType.ErrorMenu)]
public class UIErrorMenu : UIMenu
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Message;

	MenuProcessor m_MenuProcessor;

	Action m_Action;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	public void Setup(string _Title, string _Message, Action _Action = null)
	{
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
}