using System;
using UnityEngine;

[Menu(MenuType.ProductMenu)]
public class UIProductMenu : UIDialog
{
	[SerializeField] UIProductImage    m_Image;
	[SerializeField] UIProductDiscount m_Discount;
	[SerializeField] UIProductCoins    m_Coins;
	[SerializeField] UIProductPrice    m_Price;
	[SerializeField] UIProductButton   m_Button;

	Action<bool> m_Purchase;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.OnPurchase.AddListener(InvokePurchase);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.OnPurchase.RemoveListener(InvokePurchase);
	}

	public void Setup(string _ProductID, Action<bool> _Purchase = null)
	{
		m_Image.ProductID    = _ProductID;
		m_Discount.ProductID = _ProductID;
		m_Coins.ProductID    = _ProductID;
		m_Price.ProductID    = _ProductID;
		m_Button.ProductID   = _ProductID;
		m_Purchase           = _Purchase;
	}

	void InvokePurchase(bool _Success)
	{
		Action<bool> action = m_Purchase;
		m_Purchase = null;
		action?.Invoke(_Success);
	}
}
