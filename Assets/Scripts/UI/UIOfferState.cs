using TMPro;
using UnityEngine;
using Zenject;

public class UIOfferState : UIEntity
{
	public string OfferID
	{
		get => m_OfferID;
		set
		{
			if (m_OfferID == value)
				return;
			
			m_OffersManager.UnsubscribeCollect(m_OfferID, ProcessState);
			
			m_OfferID = value;
			
			m_OffersManager.SubscribeCollect(m_OfferID, ProcessState);
			
			ProcessState();
		}
	}

	[SerializeField] TMP_Text m_State;

	[Inject] OffersManager m_OffersManager;
	[Inject] Localization  m_Localization;

	string m_OfferID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		OfferID = null;
	}

	void ProcessState()
	{
		int source = m_OffersManager.GetSource(m_OfferID);
		int target = m_OffersManager.GetTarget(m_OfferID);
		
		if (m_OffersManager.Contains(m_OfferID))
			m_State.text = m_Localization.Get("OFFER_COLLECTED");
		else if (source < target)
			m_State.text = m_Localization.Format("OFFER_PROGRESS", source, target);
		else
			m_State.text = m_Localization.Get("OFFER_COLLECT");
	}
}
