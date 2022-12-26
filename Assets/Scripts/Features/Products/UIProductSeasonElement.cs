using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProductSeasonElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductSeasonElement> { }

	[SerializeField] UIProductImage  m_Image;
	[SerializeField] UIProductPrice  m_Price;
	[SerializeField] UIProductAction m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _ProductID)
	{
		m_Image.ProductID  = _ProductID;
		m_Price.ProductID  = _ProductID;
		m_Action.ProductID = _ProductID;
		
		m_BadgeManager.ReadProduct(_ProductID);
	}
}
