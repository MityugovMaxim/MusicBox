using UnityEngine;

public class UIOfferImage : UIOfferEntity
{
	[SerializeField] WebGraphic m_Image;

	protected override void Subscribe()
	{
		OffersManager.Collection.Subscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		OffersManager.Collection.Unsubscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Image.Path = OffersManager.GetImage(OfferID);
	}
}
