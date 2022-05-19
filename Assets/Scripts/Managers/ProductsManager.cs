using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProductsManager
{
	[Inject] ProductsProcessor m_ProductsProcessor;
	[Inject] ProfileProcessor  m_ProfileProcessor;

	public List<string> GetProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetSpecialProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(m_ProductsProcessor.IsSpecial)
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetPromoProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(m_ProductsProcessor.IsPromo)
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetDiscountProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(_ProductID => !m_ProductsProcessor.IsPromo(_ProductID))
			.Where(_ProductID => !m_ProductsProcessor.IsSpecial(_ProductID))
			.Where(_ProductID => !Mathf.Approximately(0, m_ProductsProcessor.GetDiscount(_ProductID)))
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetAvailableProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(_ProductID => !m_ProductsProcessor.IsPromo(_ProductID))
			.Where(_ProductID => !m_ProductsProcessor.IsSpecial(_ProductID))
			.Where(_ProductID => Mathf.Approximately(0, m_ProductsProcessor.GetDiscount(_ProductID)))
			.Where(IsAvailable)
			.ToList();
	}

	public bool IsAvailable(string _ProductID)
	{
		return !m_ProfileProcessor.HasProduct(_ProductID);
	}
}