using System;
using System.Collections.Generic;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.InterstitialMenu)]
public class UIInterstitialMenu : UIMenu
{
	const string DEFAULT_ID = "BANNER";

	[SerializeField] UIInterstitialItem m_Item;
	[SerializeField] RectTransform      m_Container;

	[Inject] BannersProcessor m_BannersProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	readonly List<UIInterstitialItem> m_Items = new List<UIInterstitialItem>();

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.InterstitialMenu);
	}

	public void Add()
	{
		UIInterstitialItem item = Instantiate(m_Item, m_Container, false);
		
		if (item == null)
			return;
		
		BannerSnapshot snapshot = m_BannersProcessor.CreateSnapshot(DEFAULT_ID);
		
		string bannerID = snapshot.ID;
		
		item.Setup(bannerID, Open, Remove);
		
		m_Items.Insert(0, item);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_BannersProcessor.Load();
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_BannersProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload banners failed.");
			
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
		foreach (UIInterstitialItem item in m_Items)
			Destroy(item.gameObject);
		m_Items.Clear();
		
		List<string> bannerIDs = m_BannersProcessor.GetBannerIDs();
		
		if (bannerIDs == null)
			return;
		
		foreach (string bannerID in bannerIDs)
		{
			UIInterstitialItem item = Instantiate(m_Item, m_Container, false);
			
			if (item == null)
				continue;
			
			item.Setup(bannerID, Open, Remove);
			
			m_Items.Add(item);
		}
	}

	async void Open(string _BannerID)
	{
		UIInterstitialSettingsMenu interstitialSettingsMenu = m_MenuProcessor.GetMenu<UIInterstitialSettingsMenu>();
		
		interstitialSettingsMenu.Setup(_BannerID);
		
		await m_MenuProcessor.Show(MenuType.InterstitialSettingsMenu);
		await m_MenuProcessor.Hide(MenuType.InterstitialMenu, true);
	}

	void Remove(string _BannerID)
	{
		m_BannersProcessor.RemoveSnapshot(_BannerID);
		
		bool Predicate(UIInterstitialItem _Item) => _Item.BannerID == _BannerID;
		
		List<UIInterstitialItem> items = m_Items.FindAll(Predicate);
		
		m_Items.RemoveAll(Predicate);
		
		foreach (UIInterstitialItem item in items)
			Destroy(item.gameObject);
	}
}