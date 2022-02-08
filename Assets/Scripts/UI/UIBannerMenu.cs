using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.BannerMenu)]
public class UIBannerMenu : UIMenu
{
	[SerializeField] UIBannerItem m_BannerItem;

	readonly HashSet<string> m_BannerIDs = new HashSet<string>();

	SocialProcessor      m_SocialProcessor;
	ApplicationProcessor m_ApplicationProcessor;
	UrlProcessor         m_UrlProcessor;
	HapticProcessor      m_HapticProcessor;
	StatisticProcessor   m_StatisticProcessor;

	[Inject]
	public void Construct(
		SocialProcessor      _SocialProcessor,
		ApplicationProcessor _ApplicationProcessor,
		UrlProcessor         _UrlProcessor,
		HapticProcessor      _HapticProcessor,
		StatisticProcessor   _StatisticProcessor
	)
	{
		m_SocialProcessor      = _SocialProcessor;
		m_ApplicationProcessor = _ApplicationProcessor;
		m_UrlProcessor         = _UrlProcessor;
		m_HapticProcessor      = _HapticProcessor;
		m_StatisticProcessor   = _StatisticProcessor;
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
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		string url = m_ApplicationProcessor.GetURL(_BannerID);
		
		await m_UrlProcessor.ProcessURL(url);
		
		ViewBanner(_BannerID);
	}

	void CloseBanner(string _BannerID)
	{
		m_StatisticProcessor.LogBannerMenuCloseClick(_BannerID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
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