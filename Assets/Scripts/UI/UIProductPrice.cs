using TMPro;
using UnityEngine;
using Zenject;

public class UIProductPrice : UIEntity
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			m_ProductID = value;
			
			ProcessPrice();
		}
	}

	[SerializeField] TMP_Text m_Price;
	[SerializeField] UIGroup  m_PriceGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] StoreProcessor m_StoreProcessor;

	string m_ProductID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		ProductID = null;
	}

	async void ProcessPrice()
	{
		m_LoaderGroup.Show(true);
		
		await m_StoreProcessor.Load();
		
		await m_LoaderGroup.HideAsync();
		
		await m_PriceGroup.ShowAsync();
		
		string price = m_StoreProcessor.GetPrice(ProductID);
		
		if (!m_Price.font.HasCharacters(price))
			price = m_StoreProcessor.GetPrice(ProductID, false);
		
		m_Price.text = price;
	}
}
