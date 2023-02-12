using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

public class UIAdminElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIAdminElement> { }

	[SerializeField] TMP_Text m_Title;

	string m_Path;
	string m_Descriptors;
	Type   m_Type;
	Action m_Action;

	public void Setup(string _Title, Action _Action)
	{
		m_Action = _Action;
		
		m_Title.text = _Title;
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		m_Action?.Invoke();
	}
}
