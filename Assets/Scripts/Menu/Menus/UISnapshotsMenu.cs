using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.SnapshotsMenu)]
public class UISnapshotsMenu : UIMenu
{
	[SerializeField] UISnapshotsList m_List;
	[SerializeField] Button          m_BackButton;
	[SerializeField] Button          m_UploadButton;
	[SerializeField] Button          m_RestoreButton;
	[SerializeField] Button          m_AddButton;

	[Inject] MenuProcessor m_MenuProcessor;

	string            m_Path;
	Type              m_Type;
	DatabaseReference m_Data;

	readonly List<Snapshot> m_Snapshots = new List<Snapshot>();

	protected override void Awake()
	{
		base.Awake();
		
		m_BackButton.onClick.AddListener(Back);
		m_UploadButton.onClick.AddListener(Upload);
		m_RestoreButton.onClick.AddListener(Restore);
		m_AddButton.onClick.AddListener(Add);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.onClick.RemoveListener(Back);
		m_UploadButton.onClick.RemoveListener(Upload);
		m_RestoreButton.onClick.RemoveListener(Restore);
		m_AddButton.onClick.RemoveListener(Add);
	}

	public void Setup(string _Path, Type _Type)
	{
		m_Path = _Path;
		m_Type = _Type;
		
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		m_Data = FirebaseDatabase.DefaultInstance.RootReference.Child(m_Path);
		
		m_Data.ValueChanged += OnUpdate;
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		m_Snapshots.Clear();
		
		m_List.Setup(null, null);
		
		if (m_Data == null)
			return;
		
		m_Data.ValueChanged -= OnUpdate;
		m_Data              =  null;
	}

	void OnUpdate(object _Sender, ValueChangedEventArgs _EventArgs)
	{
		m_Snapshots.Clear();
		
		m_List.Setup(null, null);
		
		if (_EventArgs.DatabaseError != null)
		{
			Log.Error(this, _EventArgs.DatabaseError.Message);
			return;
		}
		
		m_Snapshots.AddRange(_EventArgs.Snapshot.Children.Select(_Data => Activator.CreateInstance(m_Type, _Data) as Snapshot));
		
		m_Snapshots.Sort((_A, _B) => _A.Order.CompareTo(_B.Order));
		
		m_List.Setup(m_Path, m_Snapshots);
	}

	void Back()
	{
		Hide();
	}

	async void Upload()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"upload",
			"UPLOAD",
			"Are you sure want to upload data?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (Snapshot snapshot in m_Snapshots)
		{
			if (snapshot == null)
				continue;
			
			Dictionary<string, object> entry = new Dictionary<string, object>();
			
			snapshot.Serialize(entry);
			
			data[snapshot.ID] = entry;
		}
		
		try
		{
			await m_Data.SetValueAsync(data);
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async void Restore()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"restore",
			"RESTORE",
			"Are you sure want to restore data?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		m_Snapshots.Clear();
		
		m_List.Setup(null, null);
		
		try
		{
			DataSnapshot snapshot = await m_Data.GetValueAsync();
			
			m_Snapshots.AddRange(snapshot.Children.Select(_Data => Activator.CreateInstance(m_Type, _Data) as Snapshot));
			
			m_Snapshots.Sort((_A, _B) => _A.Order.CompareTo(_B.Order));
			
			m_List.Setup(m_Path, m_Snapshots);
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void Add()
	{
		m_Snapshots.Add(Activator.CreateInstance(m_Type) as Snapshot);
		
		m_Snapshots.Sort((_A, _B) => _A.Order.CompareTo(_B.Order));
		
		m_List.Setup(m_Path, m_Snapshots);
	}
}