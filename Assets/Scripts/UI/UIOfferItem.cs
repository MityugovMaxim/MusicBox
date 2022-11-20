using UnityEngine;
using UnityEngine.Scripting;

public class UIOfferItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIOfferItem> { }

	[SerializeField] UIOfferImage  m_Image;
	[SerializeField] UIOfferLabel  m_Label;
	[SerializeField] UIOfferState  m_State;
	[SerializeField] UIOfferAction m_Action;

	public void Setup(string _OfferID)
	{
		m_Image.OfferID  = _OfferID;
		m_Label.OfferID  = _OfferID;
		m_State.OfferID  = _OfferID;
		m_Action.OfferID = _OfferID;
	}
}
