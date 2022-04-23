using UnityEngine;

public class UIBannerImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	string m_BannerID;

	public void Setup(string _BannerID)
	{
		m_BannerID = _BannerID;
		
		m_Image.Path = $"Thumbnails/Banners/{m_BannerID}.jpg";
	}
}