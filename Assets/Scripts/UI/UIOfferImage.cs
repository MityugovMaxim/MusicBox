using UnityEngine;
using Zenject;

public class UIOfferImage : UIEntity
{
	public string OfferID
	{
		get => m_OfferID;
		set
		{
			if (m_OfferID == value)
				return;
			
			m_OffersManager.Collection.Unsubscribe(DataEventType.Change, m_OfferID, ProcessImage);
			
			m_OfferID = value;
			
			m_OffersManager.Collection.Subscribe(DataEventType.Change, m_OfferID, ProcessImage);
			
			ProcessImage();
		}
	}

	[SerializeField] WebImage m_Image;

	[Inject] OffersManager m_OffersManager;

	string m_OfferID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		OfferID = null;
	}

	void ProcessImage()
	{
		m_Image.Path = m_OffersManager.GetImage(OfferID);
	}
}
