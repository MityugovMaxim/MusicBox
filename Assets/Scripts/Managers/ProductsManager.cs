using System.Collections.Generic;
using System.Linq;
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

	public long GetCoins(string _ProductID)
	{
		long coins = m_ProductsProcessor.GetCoins(_ProductID);
		
		return m_ProfileProcessor.ApplyVoucher(ProfileVoucherType.ProductDiscount, _ProductID, coins);
	}

	public List<string> GetAvailableProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(_ProductID => !m_ProductsProcessor.IsPromo(_ProductID))
			.Where(_ProductID => !m_ProductsProcessor.IsSpecial(_ProductID))
			.Where(IsAvailable)
			.OrderBy(m_ProductsProcessor.GetCoins)
			.ToList();
	}

	public List<string> GetRecommendedProductIDs(int _Count)
	{
		List<string> productIDs = m_ProductsProcessor.GetProductIDs()
			.Where(_ProductID => !m_ProductsProcessor.IsPromo(_ProductID))
			.Where(_ProductID => !m_ProductsProcessor.IsSpecial(_ProductID))
			.Where(IsAvailable)
			.OrderBy(GetCoins)
			.ToList();
		
		int skip = 0;
		while (skip < productIDs.Count - _Count)
		{
			if (GetCoins(productIDs[skip]) >= m_ProfileProcessor.Coins)
				break;
			
			skip++;
		}
		
		return productIDs.Skip(skip).Take(_Count).ToList();
	}

	public bool IsAvailable(string _ProductID)
	{
		return !m_ProfileProcessor.HasProduct(_ProductID);
	}
}
