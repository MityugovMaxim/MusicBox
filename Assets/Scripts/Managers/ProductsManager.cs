using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProductsManager
{
	[Inject] StoreProcessor    m_StoreProcessor;
	[Inject] ProductsProcessor m_ProductsProcessor;
	[Inject] ProfileProcessor  m_ProfileProcessor;

	public List<string> GetAvailableProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(m_StoreProcessor.HasProduct)
			.Where(_ProductID => !m_ProfileProcessor.HasProduct(_ProductID))
			.OrderByDescending(_ProductID => Mathf.Abs(m_ProductsProcessor.GetDiscount(_ProductID)))
			.ToList();
	}
}