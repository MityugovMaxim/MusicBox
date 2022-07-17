using UnityEngine;
using Zenject;

public class UIBannerImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	[Inject] BannersProcessor m_BannersProcessor;

	string m_BannerID;

	public void Setup(string _BannerID)
	{
		m_BannerID = _BannerID;
		
		m_Image.Path = m_BannersProcessor.GetImage(m_BannerID);
	}
}