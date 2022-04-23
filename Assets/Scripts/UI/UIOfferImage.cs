using UnityEngine;

public class UIOfferImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	string m_OfferID;

	public void Setup(string _OfferID)
	{
		m_OfferID = _OfferID;
		
		m_Image.Path = $"Thumbnails/Offers/{m_OfferID}.jpg";
	}
}