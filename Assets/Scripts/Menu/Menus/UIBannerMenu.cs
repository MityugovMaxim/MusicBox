using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.BannerMenu)]
public class UIBannerMenu : UIMenu
{
	[SerializeField] UIBannerItem m_BannerItem;

	[Inject] SocialProcessor      m_SocialProcessor;
	[Inject] BannersProcessor     m_BannersProcessor;
	[Inject] UrlProcessor         m_UrlProcessor;
	[Inject] ApplicationManager m_ApplicationManager;

	readonly HashSet<string> m_BannerIDs = new HashSet<string>();

	public async Task Process()
	{
		List<string> bannerIDs = m_BannersProcessor.GetBannerIDs();
		
		if (bannerIDs == null || bannerIDs.Count == 0)
			return;
		
		foreach (string bannerID in bannerIDs)
		{
			if (CheckBanner(bannerID))
				continue;
			
			m_BannerItem.Setup(bannerID);
			
			await m_BannerItem.ShowAsync();
			
			UIBannerItem.BannerState state = await m_BannerItem.Process();
			
			switch (state)
			{
				case UIBannerItem.BannerState.None:
					await UnityTask.While(() => true);
					break;
				
				case UIBannerItem.BannerState.Open:
					ViewBanner(bannerID);
					string url = m_BannersProcessor.GetURL(bannerID);
					await m_BannerItem.HideAsync();
					await m_UrlProcessor.ProcessURL(url);
					return;
				
				case UIBannerItem.BannerState.Close:
					ViewBanner(bannerID);
					await m_BannerItem.HideAsync();
					break;
			}
		}
	}

	protected override void OnShowStarted()
	{
		m_BannerItem.Hide(true);
	}

	bool CheckBanner(string _BannerID)
	{
		if (m_BannersProcessor.IsPermanent(_BannerID) && m_ApplicationManager.ClientVersion == m_ApplicationManager.ServerVersion)
			return true;
		
		string userID = m_SocialProcessor.UserID;
		
		return m_BannerIDs.Contains(_BannerID) || PlayerPrefs.HasKey($"BANNER_{userID}_{_BannerID}");
	}

	void ViewBanner(string _BannerID)
	{
		string userID = m_SocialProcessor.UserID;
		
		m_BannerIDs.Add(_BannerID);
		
		PlayerPrefs.SetInt($"BANNER_{userID}_{_BannerID}", 1);
	}
}