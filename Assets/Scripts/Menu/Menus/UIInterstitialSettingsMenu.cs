using System;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.InterstitialSettingsMenu)]
public class UIInterstitialSettingsMenu : UIMenu
{
	[SerializeField] UIImageField       m_Image;
	[SerializeField] UISerializedObject m_Fields;
	[SerializeField] ScrollRect         m_Scroll;

	[Inject] BannersProcessor m_BannersProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	string         m_BannerID;
	BannerSnapshot m_Snapshot;

	public void Setup(string _BannerID)
	{
		m_BannerID = _BannerID;
		
		m_Scroll.verticalNormalizedPosition = 1;
		
		m_Image.Label = "Banner";
		m_Image.Setup($"Thumbnails/Banners/{m_BannerID}.jpg");
		
		m_Fields.Clear();
		
		m_Snapshot = m_BannersProcessor.GetSnapshot(m_BannerID);
		
		if (m_Snapshot == null)
			return;
		
		m_Fields.Add($"Interstitial: {m_BannerID}", m_Snapshot);
	}

	public async void Back()
	{
		await m_MenuProcessor.Show(MenuType.InterstitialMenu, true);
		await m_MenuProcessor.Hide(MenuType.InterstitialSettingsMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_BannersProcessor.Load();
		
		Setup(m_BannerID);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_BannersProcessor.Upload(m_BannerID, m_Snapshot.ID);
			
			Setup(m_Snapshot.ID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload banner failed.");
			
			await m_MenuProcessor.ExceptionAsync("Upload failed", exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}