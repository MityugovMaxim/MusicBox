using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UIColorsElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIColorsElement> { }

	[SerializeField] TMP_Text m_ID;
	[SerializeField] Graphic  m_BackgroundPrimary;
	[SerializeField] Graphic  m_BackgroundSecondary;
	[SerializeField] Graphic  m_ForegroundPrimary;
	[SerializeField] Graphic  m_ForegroundSecondary;
	[SerializeField] Button   m_SelectButton;
	[SerializeField] Button   m_RemoveButton;

	[Inject] MenuProcessor m_MenuProcessor;

	ColorsSnapshot         m_Snapshot;
	Action<ColorsSnapshot> m_Select;
	Action<ColorsSnapshot> m_Remove;

	protected override void Awake()
	{
		base.Awake();
		
		m_SelectButton.onClick.AddListener(Select);
		m_RemoveButton.onClick.AddListener(Remove);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SelectButton.onClick.RemoveListener(Select);
		m_RemoveButton.onClick.RemoveListener(Remove);
	}

	public void Setup(ColorsSnapshot _Snapshot, Action<ColorsSnapshot> _Select, Action<ColorsSnapshot> _Remove)
	{
		m_Snapshot = _Snapshot;
		m_Select   = _Select;
		m_Remove   = _Remove;
		
		m_SelectButton.gameObject.SetActive(m_Select != null);
		
		Refresh();
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	void Open()
	{
		UIColorsEditMenu colorsEditMenu = m_MenuProcessor.GetMenu<UIColorsEditMenu>();
		
		if (colorsEditMenu == null)
			return;
		
		colorsEditMenu.Setup(m_Snapshot, Refresh);
		
		colorsEditMenu.Show();
	}

	void Refresh()
	{
		m_ID.text = m_Snapshot.ID;
		
		m_BackgroundPrimary.color   = m_Snapshot.BackgroundPrimary;
		m_BackgroundSecondary.color = m_Snapshot.BackgroundSecondary;
		m_ForegroundPrimary.color   = m_Snapshot.ForegroundPrimary;
		m_ForegroundSecondary.color = m_Snapshot.ForegroundSecondary;
	}

	void Select()
	{
		m_Select?.Invoke(m_Snapshot);
	}

	void Remove()
	{
		m_Remove?.Invoke(m_Snapshot);
	}
}