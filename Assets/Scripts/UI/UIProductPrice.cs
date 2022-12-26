using TMPro;
using UnityEngine;

public class UIProductPrice : UIProductEntity
{
	[SerializeField] TMP_Text m_Price;
	[SerializeField] UIGroup  m_PriceGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	protected override void Subscribe()
	{
		ProductsManager.Collection.Subscribe(DataEventType.Change, ProductID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ProductsManager.Collection.Unsubscribe(DataEventType.Change, ProductID, ProcessData);
	}

	protected override async void ProcessData()
	{
		m_LoaderGroup.Show(true);
		
		bool instant = await ProductsManager.Activate();
		
		m_LoaderGroup.Hide(instant);
		m_PriceGroup.Show(instant);
		
		string price = ProductsManager.GetPriceSign(ProductID);
		
		if (!m_Price.font.HasCharacters(price))
			price = ProductsManager.GetPriceCode(ProductID);
		
		m_Price.text = price;
	}
}
