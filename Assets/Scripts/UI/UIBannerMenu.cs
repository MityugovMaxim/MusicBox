using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.BannerMenu)]
public class UIBannerMenu : UIMenu
{
	[SerializeField] UIBannerItem m_BannerItem;

	ApplicationProcessor m_ApplicationProcessor;
	UrlProcessor         m_UrlProcessor;

	[Inject]
	public void Construct(
		ApplicationProcessor _ApplicationProcessor,
		UrlProcessor         _UrlProcessor
	)
	{
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
				
				break;
			}
			
			await m_BannerItem.HideAsync();
		}
	}

	protected override void OnShowStarted()
	{
		m_BannerItem.Hide(true);
	}
}