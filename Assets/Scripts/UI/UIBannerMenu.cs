using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.BannerMenu)]
public class UIBannerMenu : UIMenu
{
	[SerializeField] UIBannerItem m_BannerItem;

	[Inject] SocialProcessor    m_SocialProcessor;
	[Inject] BannersProcessor   m_BannersProcessor;
	[Inject] UrlProcessor       m_UrlProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

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
			
			bool permanent = m_BannersProcessor.CheckPermanent(bannerID);
			
			m_BannerItem.Setup(bannerID);
			
			await m_BannerItem.ShowAsync();
			
			UIBannerItem.BannerState state = await m_BannerItem.Process(permanent);
			
			if (state == UIBannerItem.BannerState.Open)
			{
				await OpenBanner(bannerID);
				break;
			}
			else
			{
				CloseBanner(bannerID);
			}
			
			await UnityTask.While(() => permanent);
			
			await m_BannerItem.HideAsync();
		}
	}

	async Task OpenBanner(string _BannerID)
	{
		m_StatisticProcessor.LogBannerMenuOpenClick(_BannerID);
		
		string url = m_BannersProcessor.GetURL(_BannerID);
		
		await m_UrlProcessor.ProcessURL(url);
		
		ViewBanner(_BannerID);
	}

	void CloseBanner(string _BannerID)
	{
		m_StatisticProcessor.LogBannerMenuCloseClick(_BannerID);
		
		ViewBanner(_BannerID);
	}

	protected override void OnShowStarted()
	{
		m_BannerItem.Hide(true);
	}

	bool CheckBanner(string _BannerID)
	{
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