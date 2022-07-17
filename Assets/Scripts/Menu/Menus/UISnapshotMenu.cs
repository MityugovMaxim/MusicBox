using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.SnapshotMenu)]
public class UISnapshotMenu : UIMenu
{
	[SerializeField] RectTransform m_Container;
	[SerializeField] Button        m_BackButton;
	[SerializeField] Button        m_RestoreButton;
	[SerializeField] Button        m_UploadButton;

	[Inject] UIField.Factory m_Factory;
	[Inject] MenuProcessor   m_MenuProcessor;

	readonly List<UIField> m_Items = new List<UIField>();

	string   m_Path;
	string   m_SnapshotID;
	Snapshot m_Snapshot;

	protected override void Awake()
	{
		base.Awake();
		
		m_BackButton.onClick.AddListener(Back);
		m_RestoreButton.onClick.AddListener(Restore);
		m_UploadButton.onClick.AddListener(Upload);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.onClick.RemoveListener(Back);
		m_RestoreButton.onClick.RemoveListener(Restore);
		m_UploadButton.onClick.RemoveListener(Upload);
	}

	public void Setup(string _Path, Snapshot _Snapshot)
	{
		m_Path       = _Path;
		m_Snapshot   = _Snapshot;
		m_SnapshotID = _Snapshot.ID;
	}

	void Back()
	{
		Hide();
	}

	async void Upload()
	{
		string snapshotID = m_Snapshot.ID;
		
		if (string.IsNullOrEmpty(snapshotID))
			return;
		
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"upload",
			$"UPLOAD {m_Snapshot.ID}",
			"Are you sure want to upload data?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference
			.Child(m_Path)
			.Child(snapshotID);
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		m_Snapshot.Serialize(data);
		
		try
		{
			if (m_SnapshotID != snapshotID)
			{
				await DeleteAsync(m_SnapshotID);
				
				m_SnapshotID = snapshotID;
			}
			
			await reference.SetValueAsync(data);
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async void Restore()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"restore",
			$"RESTORE {m_Snapshot.ID}",
			"Are you sure want to restore data?"
		);
		
		if (!confirm)
			return;
		
		foreach (UIField item in m_Items)
			item.Restore();
	}

	Task DeleteAsync(string _SnapshotID)
	{
		if (string.IsNullOrEmpty(_SnapshotID))
			return Task.CompletedTask;
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference
			.Child(m_Path)
			.Child(_SnapshotID);
		
		return reference.SetValueAsync(null);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Refresh();
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		Clear();
	}

	void Clear()
	{
		foreach (UIField item in m_Items)
			Destroy(item.gameObject);
		m_Items.Clear();
	}

	void Refresh()
	{
		Clear();
		
		Type type = m_Snapshot.GetType();
		
		PropertyInfo[] propertyInfos = type.GetProperties(
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.DeclaredOnly |
			BindingFlags.Instance |
			BindingFlags.Static |
			BindingFlags.DeclaredOnly |
			BindingFlags.FlattenHierarchy
		);
		
		PropertyInfo[] basePropertyInfos = type.GetProperties()
			.Except(propertyInfos)
			.ToArray();
		
		foreach (PropertyInfo propertyInfo in basePropertyInfos)
		{
			UIField item = m_Factory.Create(m_Snapshot, propertyInfo, m_Container);
			
			if (item == null)
				continue;
			
			m_Items.Add(item);
		}
		
		foreach (PropertyInfo propertyInfo in propertyInfos)
		{
			UIField item = m_Factory.Create(m_Snapshot, propertyInfo, m_Container);
			
			if (item == null)
				continue;
			
			m_Items.Add(item);
		}
	}
}