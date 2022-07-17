using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIAdminElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIAdminElement> { }

	[SerializeField] TMP_Text m_Title;

	[Inject] MenuProcessor m_MenuProcessor;

	string m_Path;
	string m_Descriptors;
	Type   m_Type;
	Action m_Action;

	public void Setup(
		string _Title,
		string _Path,
		string _Descriptors,
		Type   _Type,
		Action _Action = null
	)
	{
		m_Path        = _Path;
		m_Descriptors = _Descriptors;
		m_Type        = _Type;
		m_Action      = _Action;
		
		m_Title.text = _Title;
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	void Open()
	{
		if (m_Action != null)
		{
			m_Action();
			return;
		}
		
		UISnapshotsMenu snapshotsMenu = m_MenuProcessor.GetMenu<UISnapshotsMenu>();
		
		if (snapshotsMenu == null)
			return;
		
		snapshotsMenu.Setup(m_Path, m_Descriptors, m_Type);
		
		snapshotsMenu.Show();
	}
}