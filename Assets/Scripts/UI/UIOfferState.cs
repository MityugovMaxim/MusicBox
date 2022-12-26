using TMPro;
using UnityEngine;
using Zenject;

public class UIOfferState : UIOfferEntity
{
	[SerializeField] TMP_Text m_State;
	[SerializeField] TMP_Text m_Progress;
	[SerializeField] UIGroup  m_StateGroup;
	[SerializeField] UIGroup  m_ProgressGroup;

	[Inject] Localization m_Localization;

	protected override void Subscribe()
	{
		OffersManager.SubscribeCollect(OfferID, ProcessData);
		OffersManager.Profile.Subscribe(DataEventType.Add, OfferID, ProcessData);
		OffersManager.Profile.Subscribe(DataEventType.Remove, OfferID, ProcessData);
		OffersManager.Collection.Subscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		OffersManager.UnsubscribeCollect(OfferID, ProcessData);
		OffersManager.Profile.Unsubscribe(DataEventType.Add, OfferID, ProcessData);
		OffersManager.Profile.Unsubscribe(DataEventType.Remove, OfferID, ProcessData);
		OffersManager.Collection.Unsubscribe(DataEventType.Change, OfferID, ProcessData);
	}

	protected override void ProcessData()
	{
		if (OffersManager.IsCollected(OfferID))
		{
			m_StateGroup.Show();
			m_ProgressGroup.Hide();
			m_State.text = m_Localization.Get("OFFER_COLLECTED");
		}
		else if (OffersManager.IsProcessing(OfferID))
		{
			m_ProgressGroup.Show();
			m_StateGroup.Hide();
			int source = OffersManager.GetSource(OfferID);
			int target = OffersManager.GetTarget(OfferID);
			m_Progress.text = $"<sprite name=ads>{source}/{target}";
		}
		else
		{
			m_StateGroup.Show();
			m_ProgressGroup.Hide();
			m_State.text = m_Localization.Get("OFFER_COLLECT");
		}
	}
}
