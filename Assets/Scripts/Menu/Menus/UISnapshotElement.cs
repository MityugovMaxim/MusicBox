using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UISnapshotElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UISnapshotElement> { }

	[SerializeField] TMP_Text m_Order;
	[SerializeField] TMP_Text m_ID;
	[SerializeField] Button   m_RemoveButton;

	[Inject] MenuProcessor m_MenuProcessor;

	string           m_Path;
	Snapshot         m_Snapshot;
	Action<Snapshot> m_Remove;

	protected override void Awake()
	{
		base.Awake();
		
		m_RemoveButton.onClick.AddListener(Remove);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_RemoveButton.onClick.RemoveListener(Remove);
	}

	public void Setup(string _Path, Snapshot _Snapshot, Action<Snapshot> _Remove)
	{
		m_Path     = _Path;
		m_Snapshot = _Snapshot;
		m_Remove   = _Remove;
		
		m_Order.text = m_Snapshot.Order.ToString();
		m_ID.text    = m_Snapshot.ID;
		
		m_RemoveButton.onClick.RemoveAllListeners();
		m_RemoveButton.onClick.AddListener(Remove);
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	async void Open()
	{
		UISnapshotMenu snapshotMenu = m_MenuProcessor.GetMenu<UISnapshotMenu>();
		
		if (snapshotMenu == null)
			return;
		
		snapshotMenu.Setup(m_Path, m_Snapshot);
		
		await m_MenuProcessor.Show(MenuType.SnapshotMenu);
	}

	async void Remove()
	{
		if (m_Remove == null)
			return;
		
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"remove",
			"REMOVE",
			"Are you sure want to remove data?"
		);
		
		if (!confirm)
			return;
		
		m_Remove?.Invoke(m_Snapshot);
	}
}