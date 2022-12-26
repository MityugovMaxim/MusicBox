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
	[SerializeField] Button       m_DistinctButton;
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
		m_DistinctButton.onClick.AddListener(Distinct);
		m_AddButton.onClick.AddListener(Add);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.onClick.RemoveListener(Back);
		m_UploadButton.onClick.RemoveListener(Upload);
		m_RestoreButton.onClick.RemoveListener(Restore);
		m_DistinctButton.onClick.RemoveListener(Distinct);
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

	void Upload()
	{
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

	void Distinct()
	{
		bool Approximately(Color _A, Color _B, float _Tolerance)
		{
			return Mathf.Abs(_A.r - _B.r) < _Tolerance &&
				Mathf.Abs(_A.g - _B.g) < _Tolerance &&
				Mathf.Abs(_A.b - _B.b) < _Tolerance &&
				Mathf.Abs(_A.a - _B.a) < _Tolerance;
		}
		
		bool Same(ColorsSnapshot _A, ColorsSnapshot _B, float _Tolerance)
		{
			return Approximately(_A.BackgroundPrimary, _B.BackgroundPrimary, _Tolerance) &&
				Approximately(_A.BackgroundSecondary, _B.BackgroundSecondary, _Tolerance) &&
				Approximately(_A.ForegroundPrimary, _B.ForegroundPrimary, _Tolerance) &&
				Approximately(_A.ForegroundSecondary, _B.ForegroundSecondary, _Tolerance);
		}
		
		List<ColorsSnapshot> snapshots = new List<ColorsSnapshot>();
		for (int i = 0; i < m_Snapshots.Count; i++)
		for (int j = i + 1; j < m_Snapshots.Count; j++)
		{
			ColorsSnapshot a = m_Snapshots[i];
			ColorsSnapshot b = m_Snapshots[j];
			
			if (Same(a, b, 0.025f))
				snapshots.Add(b);
		}
		
		foreach (ColorsSnapshot snapshot in snapshots)
			m_Snapshots.Remove(snapshot);
		
		m_List.Clear();
		
		m_List.Setup(m_Snapshots);
	}

	void Add()
	{
	}
}
