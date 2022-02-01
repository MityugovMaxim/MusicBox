using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.BannerMenu)]
public class UIBannerMenu : UIMenu
{
	[SerializeField] UIBannerItem m_BannerItem;

	SocialProcessor      m_SocialProcessor;
	ApplicationProcessor m_ApplicationProcessor;
	UrlProcessor         m_UrlProcessor;

	[Inject]
	public void Construct(
		SocialProcessor      _SocialProcessor,
		ApplicationProcessor _ApplicationProcessor,
		UrlProcessor         _UrlProcessor
	)
	{
		m_SocialProcessor      = _SocialProcessor;
		m_ApplicationProcessor = _ApplicationProcessor;
		m_UrlProcessor         = _UrlProcessor;
	}

	public async Task Process()
	{
		List<string> bannerIDs = m_ApplicationProcessor.GetBannerIDs();
		
		if (bannerIDs == null || bannerIDs.Count == 0)
			return;
		
		foreach (string bannerID in bannerIDs)
		{
			if (CheckBanner(bannerID))
				continue;
			
			bool permanent = m_ApplicationProcessor.IsPermanent(bannerID);
			
			m_BannerItem.Setup(bannerID);
			
			await m_BannerItem.ShowAsync();
			
			UIBannerItem.BannerState state = await m_BannerItem.Process(permanent);
			
			if (state == UIBannerItem.BannerState.Open)
			{
				string url = m_ApplicationProcessor.GetURL(bannerID);
				
				await m_UrlProcessor.ProcessURL(url);
				
				if (permanent)
				{
					while (true)
						await Task.Delay(10000);
				}
				
				ViewBanner(bannerID);
				
				break;
			}
			
			await m_BannerItem.HideAsync();
		}
	}

	protected override void OnShowStarted()
	{
		m_BannerItem.Hide(true);
	}

	bool CheckBanner(string _BannerID)
	{
		string userID = m_SocialProcessor.UserID;
		
		return PlayerPrefs.HasKey($"BANNER_{userID}_{_BannerID}");
	}

	void ViewBanner(string _BannerID)
	{
		string userID = m_SocialProcessor.UserID;
		
		PlayerPrefs.SetInt($"BANNER_{userID}_{_BannerID}", 1);
	}
}