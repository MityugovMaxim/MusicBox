using UnityEngine;
using Zenject;

public class UIOfferImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	[Inject] OffersProcessor m_OffersProcessor;

	string m_OfferID;

	public void Setup(string _OfferID)
	{
		m_OfferID = _OfferID;
		
		m_Image.Path = m_OffersProcessor.GetImage(m_OfferID);
	}
}