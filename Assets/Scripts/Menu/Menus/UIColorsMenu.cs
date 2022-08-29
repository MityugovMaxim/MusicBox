using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ColorsMenu)]
public class UIColorsMenu : UIMenu
{
	[SerializeField] UIColorsList m_List;
	[SerializeField] Button       m_BackButton;
	[SerializeField] Button       m_UploadButton;
	[SerializeField] Button       m_RestoreButton;
	[SerializeField] Button       m_AddButton;

	[Inject] MenuProcessor m_MenuProcessor;

	readonly List<ColorsSnapshot> m_Snapshots = new List<ColorsSnapshot>();

	DatabaseReference m_Data;

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

	protected override void OnShowStarted()
	{
		m_Snapshots.Clear();
		
		m_List.Clear();
		
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		m_Data = FirebaseDatabase.DefaultInstance.RootReference.Child("colors");
		
		m_Data.ValueChanged += OnUpdate;
	}

	void OnUpdate(object _Sender, ValueChangedEventArgs _EventArgs)
	{
		m_Snapshots.Clear();
		
		m_List.Clear();
		
		if (_EventArgs.DatabaseError != null)
		{
			Log.Error(this, _EventArgs.DatabaseError.Message);
			return;
		}
		
		m_Snapshots.AddRange(_EventArgs.Snapshot.Children.Select(_Data => new ColorsSnapshot(_Data)));
		
		m_Snapshots.Sort((_A, _B) => _A.Order.CompareTo(_B.Order));
		
		m_List.Setup(m_Snapshots);
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
			"Are you sure want to upload colors?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (ColorsSnapshot snapshot in m_Snapshots)
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
			"Are you sure want to restore colors?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		m_Snapshots.Clear();
		
		m_List.Clear();
		
		try
		{
			DataSnapshot snapshot = await m_Data.GetValueAsync();
			
			m_Snapshots.AddRange(snapshot.Children.Select(_Data => new ColorsSnapshot(_Data)));
			
			m_Snapshots.Sort((_A, _B) => _A.Order.CompareTo(_B.Order));
			
			m_List.Setup(m_Snapshots);
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void Add()
	{
		m_Snapshots.Add(new ColorsSnapshot("NEW COLOR SCHEME", 0));
		
		m_Snapshots.Sort((_A, _B) => _A.Order.CompareTo(_B.Order));
		
		m_List.Setup(m_Snapshots);
	}
}