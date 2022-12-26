using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIOfferElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIOfferElement> { }

	[SerializeField] UIOfferImage    m_Image;
	[SerializeField] UIOfferLabel    m_Label;
	[SerializeField] UIOfferReward[] m_Rewards;
	[SerializeField] UIOfferState    m_State;
	[SerializeField] UIOfferAction   m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _OfferID)
	{
		m_Image.OfferID  = _OfferID;
		m_Label.OfferID  = _OfferID;
		m_State.OfferID  = _OfferID;
		m_Action.OfferID = _OfferID;
		
		foreach (UIOfferReward reward in m_Rewards)
			reward.OfferID = _OfferID;
	}
}
