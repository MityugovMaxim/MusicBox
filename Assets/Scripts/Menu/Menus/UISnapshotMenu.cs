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
	[SerializeField] RectTransform    m_Container;
	[SerializeField] UISnapshotHeader m_Header;
	[SerializeField] Button           m_BackButton;
	[SerializeField] Button           m_RestoreButton;
	[SerializeField] Button           m_UploadButton;

	[Inject] UIField.Factory     m_Factory;
	[Inject] MenuProcessor       m_MenuProcessor;
	[Inject] LanguagesCollection m_LanguagesCollection;

	readonly List<UIField> m_Items = new List<UIField>();

	string   m_Path;
	string   m_Descriptors;
	string   m_SnapshotID;
	Snapshot m_Snapshot;

	readonly List<UISnapshotHeader>         m_Headers             = new List<UISnapshotHeader>();
	readonly Dictionary<string, Descriptor> m_DescriptorsRegistry = new Dictionary<string, Descriptor>();

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

	public void Setup(string _Path, string _Descriptors, Snapshot _Snapshot)
	{
		m_Path        = _Path;
		m_Descriptors = _Descriptors;
		m_Snapshot    = _Snapshot;
		m_SnapshotID  = _Snapshot.ID;
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
		
		try
		{
			await DeleteAsync();
			
			await UploadSnapshot();
			
			await UploadDescriptors();
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	Task UploadSnapshot()
	{
		return Task.CompletedTask;
	}

	Task UploadDescriptors()
	{
		return Task.CompletedTask;
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

	async Task DeleteAsync()
	{
		if (string.IsNullOrEmpty(m_SnapshotID) || m_SnapshotID == m_Snapshot.ID)
			return;
		
		string snapshotID = m_SnapshotID;
		
		m_SnapshotID = m_Snapshot.ID;
		
		if (!string.IsNullOrEmpty(m_Path))
		{
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference
				.Child(m_Path)
				.Child(snapshotID);
			
			await reference.SetValueAsync(null);
		}
		
		if (!string.IsNullOrEmpty(m_Descriptors))
		{
			IReadOnlyList<string> languages = m_LanguagesCollection.GetIDs();
			
			foreach (string language in languages)
			{
				if (string.IsNullOrEmpty(language))
					continue;
				
				DatabaseReference descriptorReference = FirebaseDatabase.DefaultInstance.RootReference
					.Child(m_Descriptors)
					.Child(language)
					.Child(snapshotID);
				
				await descriptorReference.SetValueAsync(null);
			}
		}
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
		
		foreach (UISnapshotHeader item in m_Headers)
			Destroy(item.gameObject);
		m_Headers.Clear();
	}

	void Refresh()
	{
		Clear();
		
		CreateSnapshot(
			m_Snapshot,
			nameof(m_Snapshot.Order)
		);
		
		CreateDescriptors();
	}

	void CreateSnapshot(Snapshot _Snapshot, params string[] _Exclude)
	{
		if (_Snapshot == null)
			return;
		
		PropertyInfo[] propertyInfos = GetProperties(_Snapshot, _Exclude);
		
		if (propertyInfos == null || propertyInfos.Length == 0)
			return;
		
		foreach (PropertyInfo propertyInfo in propertyInfos)
		{
			UIField item = m_Factory.Create(_Snapshot, propertyInfo, m_Container);
			
			if (item == null)
				continue;
			
			m_Items.Add(item);
		}
	}

	void CreateDescriptors()
	{
	}

	static PropertyInfo[] GetProperties(Snapshot _Snapshot, params string[] _Exclude)
	{
		if (_Snapshot == null)
			return null;
		
		Type type = _Snapshot.GetType();
		
		PropertyInfo[] propertyInfos = type.GetProperties(
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.DeclaredOnly |
			BindingFlags.Instance |
			BindingFlags.Static |
			BindingFlags.DeclaredOnly |
			BindingFlags.FlattenHierarchy
		);
		
		return type.GetProperties()
			.Except(propertyInfos)
			.Union(propertyInfos)
			.Where(_PropertyInfo => _Exclude == null || !_Exclude.Contains(_PropertyInfo.Name))
			.ToArray();
	}

	void CreateHeader(string _Title)
	{
		if (string.IsNullOrEmpty(_Title) || m_Header == null)
			return;
		
		UISnapshotHeader header = Instantiate(m_Header, m_Container, false);
		
		if (header == null)
			return;
		
		header.Setup(_Title);
		
		m_Headers.Add(header);
	}
}
