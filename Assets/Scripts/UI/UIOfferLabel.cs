using TMPro;
using UnityEngine;
using Zenject;

public class UIOfferLabel : UIEntity
{
	public string OfferID
	{
		get => m_OfferID;
		set
		{
			if (m_OfferID == value)
				return;
			
			m_OffersManager.Collection.Unsubscribe(DataEventType.Change, m_OfferID, ProcessLabel);
			
			m_OfferID = value;
			
			m_OffersManager.Collection.Subscribe(DataEventType.Change, m_OfferID, ProcessLabel);
			
			ProcessLabel();
		}
	}

	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	[Inject] OffersManager m_OffersManager;

	string m_OfferID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		OfferID = null;
	}

	void ProcessLabel()
	{
		m_Title.text       = m_OffersManager.GetTitle(OfferID);
		m_Description.text = m_OffersManager.GetDescription(OfferID);
	}
}
