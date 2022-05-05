using System;
using System.Collections.Generic;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.AmbientMenu)]
public class UIAmbientMenu : UIMenu
{
	const string DEFAULT_ID = "AMBIENT";

	[SerializeField] UIAmbientItem m_Item;
	[SerializeField] RectTransform      m_Container;

	[Inject] AmbientProcessor m_AmbientProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	readonly List<UIAmbientItem> m_Items = new List<UIAmbientItem>();

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.AmbientMenu);
	}

	public void Add()
	{
		UIAmbientItem item = Instantiate(m_Item, m_Container, false);
		
		if (item == null)
			return;
		
		AmbientSnapshot snapshot = m_AmbientProcessor.CreateSnapshot(DEFAULT_ID);
		
		string ambientID = snapshot.ID;
		
		item.Setup(ambientID, Open, Remove);
		
		m_Items.Insert(0, item);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AmbientProcessor.Load();
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_AmbientProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload ambient failed.");
			
			await m_MenuProcessor.ExceptionAsync("Upload failed", exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh();
	}

	void Refresh()
	{
		foreach (UIAmbientItem item in m_Items)
			Destroy(item.gameObject);
		m_Items.Clear();
		
		List<string> ambientIDs = m_AmbientProcessor.GetAmbientIDs();
		
		if (ambientIDs == null)
			return;
		
		foreach (string ambientID in ambientIDs)
		{
			UIAmbientItem item = Instantiate(m_Item, m_Container, false);
			
			if (item == null)
				continue;
			
			item.Setup(ambientID, Open, Remove);
			
			m_Items.Add(item);
		}
	}

	async void Open(string _AmbientID)
	{
		UIAmbientSettingsMenu ambientSettingsMenu = m_MenuProcessor.GetMenu<UIAmbientSettingsMenu>();
		
		ambientSettingsMenu.Setup(_AmbientID);
		
		await m_MenuProcessor.Show(MenuType.AmbientSettingsMenu);
		await m_MenuProcessor.Hide(MenuType.AmbientMenu, true);
	}

	void Remove(string _AmbientID)
	{
		m_AmbientProcessor.RemoveSnapshot(_AmbientID);
		
		bool Predicate(UIAmbientItem _Item) => _Item.AmbientID == _AmbientID;
		
		List<UIAmbientItem> items = m_Items.FindAll(Predicate);
		
		m_Items.RemoveAll(Predicate);
		
		foreach (UIAmbientItem item in items)
			Destroy(item.gameObject);
	}
}