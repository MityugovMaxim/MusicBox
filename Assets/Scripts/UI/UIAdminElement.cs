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
	Type   m_Type;

	public void Setup(string _Title, string _Path, Type _Type)
	{
		m_Path = _Path;
		m_Type = _Type;
		
		m_Title.text = _Title;
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	void Open()
	{
		UISnapshotsMenu snapshotsMenu = m_MenuProcessor.GetMenu<UISnapshotsMenu>();
		
		if (snapshotsMenu == null)
			return;
		
		snapshotsMenu.Setup(m_Path, m_Type);
		
		snapshotsMenu.Show();
	}
}