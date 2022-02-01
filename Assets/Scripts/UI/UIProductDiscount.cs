using TMPro;
using UnityEngine;
using Zenject;

public class UIProductDiscount : UIGroup
{
	[SerializeField] TMP_Text m_Label;

	ProductProcessor m_ProductProcessor;

	string m_ProductID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Hide(true);
	}

	[Inject]
	public void Construct(ProductProcessor _ProductProcessor)
	{
		m_ProductProcessor = _ProductProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		float discount = m_ProductProcessor.GetDiscount(m_ProductID);
		
		if (Mathf.Approximately(discount, 0))
			Hide();
		else
			Show();
		
		string sign = discount > 0 ? "+" : string.Empty;
		
		m_Label.text = $"{sign}{discount}%";
	}
}