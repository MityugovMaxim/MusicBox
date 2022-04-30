using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.ProgressMenu)]
public class UIProgressMenu : UIMenu
{
	[SerializeField] UIProgressItem m_Item;
	[SerializeField] RectTransform  m_Container;

	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	readonly List<UIProgressItem> m_Items = new List<UIProgressItem>();

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.ProgressMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_ProgressProcessor.Load();
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		int[] levels = m_Items
			.Where(_Item => _Item != null)
			.Select(_Item => _Item.Level)
			.ToArray();
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_ProgressProcessor.Upload(levels);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload progress failed.");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"upload_progress",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public void Add()
	{
		ProgressSnapshot snapshot = m_ProgressProcessor.CreateSnapshot();
		
		if (snapshot == null)
			return;
		
		UIProgressItem item = Instantiate(m_Item, m_Container, false);
		
		if (item == null)
			return;
		
		item.Setup(snapshot.Level, Open, Remove);
		
		m_Items.Add(item);
	}

	protected override void OnShowStarted()
	{
		Refresh();
	}

	void Refresh()
	{
		foreach (UIProgressItem item in m_Items)
			Destroy(item.gameObject);
		m_Items.Clear();
		
		List<int> levels = m_ProgressProcessor.GetLevels();
		
		if (levels == null)
			return;
		
		foreach (int level in levels)
		{
			UIProgressItem item = Instantiate(m_Item, m_Container, false);
			
			if (item == null)
				continue;
			
			item.Setup(level, Open, Remove);
			
			m_Items.Add(item);
		}
	}

	async void Open(int _Level)
	{
		UIProgressSettingsMenu progressSettingsMenu = m_MenuProcessor.GetMenu<UIProgressSettingsMenu>();
		
		progressSettingsMenu.Setup(_Level);
		
		await m_MenuProcessor.Show(MenuType.ProgressSettingsMenu);
		await m_MenuProcessor.Hide(MenuType.ProgressMenu, true);
	}

	void Remove(int _Level)
	{
		bool Predicate(UIProgressItem _Item) => _Item.Level == _Level;
		
		List<UIProgressItem> items = m_Items.FindAll(Predicate);
		
		m_Items.RemoveAll(Predicate);
		
		foreach (UIProgressItem item in items)
			Destroy(item.gameObject);
	}
}