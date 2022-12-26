using TMPro;
using UnityEngine;

public class UIOfferLabel : UIOfferEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	protected override void Subscribe()
	{
		OffersManager.Descriptor.Subscribe(DataEventType.Add, OfferID, ProcessData);
		OffersManager.Descriptor.Subscribe(DataEventType.Remove, OfferID, ProcessData);
		OffersManager.Descriptor.Subscribe(DataEventType.Change, OfferID, ProcessData);
		OffersManager.Collection.Subscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		OffersManager.Descriptor.Unsubscribe(DataEventType.Add, OfferID, ProcessData);
		OffersManager.Descriptor.Unsubscribe(DataEventType.Remove, OfferID, ProcessData);
		OffersManager.Descriptor.Unsubscribe(DataEventType.Change, OfferID, ProcessData);
		OffersManager.Collection.Unsubscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Title.text       = OffersManager.GetTitle(OfferID);
		m_Description.text = OffersManager.GetDescription(OfferID);
	}
}
