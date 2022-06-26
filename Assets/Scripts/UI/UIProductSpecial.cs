using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProductSpecial : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductSpecial> { }

	[SerializeField] UIProductImage m_Image;
	[SerializeField] UIProductLabel m_Label;
	[SerializeField] UIProductPrice m_Price;

	[Inject] MenuProcessor m_MenuProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(m_ProductID);
		productMenu.Show();
	}
}