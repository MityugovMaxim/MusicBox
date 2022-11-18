using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProductsManager : ProfileCollection<PurchaseSnapshot>
{
	public ProductsCollection Collection => m_ProductsCollection;
	public ProductsDescriptor Descriptor => m_ProductsDescriptor;

	protected override string Name => "products";

	bool Processing { get; set; }

	[Inject] ProductsCollection m_ProductsCollection;
	[Inject] ProductsDescriptor m_ProductsDescriptor;
	[Inject] StoreProcessor     m_StoreProcessor;
	[Inject] VouchersManager    m_VouchersManager;
	[Inject] CoinsParameter     m_CoinsParameter;
	[Inject] MenuProcessor      m_MenuProcessor;

	public async Task Purchase(string _ProductID)
	{
		if (Processing)
			return;
		
		Processing = true;
		
		#if UNITY_EDITOR
		await Task.Delay(1500);
		#else
		try
		{
			await m_StoreProcessor.Purchase(_ProductID);
		}
		finally
		{
			Processing = false;
		}
		#endif
	}

	public string GetProductID(long _Coins)
	{
		return m_ProductsCollection.GetIDs()
			.OrderByDescending(m_VouchersManager.GetProductDiscount)
			.Select(m_ProductsCollection.GetSnapshot)
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.Aggregate(
				(_A, _B) =>
				{
					long aCoins = m_VouchersManager.GetProductDiscount(_A);
					long bCoins = m_VouchersManager.GetProductDiscount(_B);
					return aCoins >= _Coins && aCoins < bCoins ? _A : _B;
				}
			);
	}

	public List<string> GetProductIDs()
	{
		return m_ProductsCollection.GetIDs()
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetSpecialProductIDs()
	{
		return m_ProductsCollection.GetIDs()
			.Where(IsAvailable)
			.Where(IsSpecial)
			.ToList();
	}

	public List<string> GetPromoProductIDs()
	{
		return m_ProductsCollection.GetIDs()
			.Where(IsAvailable)
			.Where(IsPromo)
			.ToList();
	}

	public List<string> GetAvailableProductIDs()
	{
		return m_ProductsCollection.GetIDs()
			.Where(IsAvailable)
			.Where(IsRegular)
			.OrderBy(GetCoins)
			.ToList();
	}

	public List<string> GetRecommendedProductIDs(int _Count)
	{
		List<string> productIDs = m_ProductsCollection.GetIDs()
			.Where(IsAvailable)
			.Where(IsRegular)
			.OrderBy(m_VouchersManager.GetProductDiscount)
			.ToList();
		
		int skip = 0;
		while (skip < productIDs.Count - _Count)
		{
			string productID = productIDs[skip];
			
			long coins = m_VouchersManager.GetProductDiscount(productID);
			
			if (coins >= m_CoinsParameter.Value)
				break;
			
			skip++;
		}
		
		return productIDs.Skip(skip).Take(_Count).ToList();
	}

	public IDs GetStoreIDs(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		if (snapshot == null)
			return null;
		
		return new IDs()
		{
			{ snapshot.AppStoreID, AppleAppStore.Name },
			{ snapshot.GooglePlayID, GooglePlay.Name },
		};
	}

	public string GetImage(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _ProductID) => m_ProductsDescriptor.GetTitle(_ProductID);

	public string GetDescription(string _ProductID) => m_ProductsDescriptor.GetDescription(_ProductID);

	public ProductType GetType(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Type ?? ProductType.Consumable;
	}

	public long GetCoins(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Coins ?? 0;
	}

	public List<string> GetSongIDs(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.SongIDs != null
			? snapshot.SongIDs.ToList()
			: new List<string>();
	}

	public bool IsRegular(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot != null && !snapshot.Promo && !snapshot.Special;
	}

	public bool IsPromo(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Promo ?? false;
	}

	public bool IsSpecial(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Special ?? false;
	}

	public bool IsBattlePass(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.BattlePass ?? false;
	}

	bool IsAvailable(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		if (snapshot == null || !snapshot.Active)
			return false;
		
		if (snapshot.Type == ProductType.NonConsumable)
			return !Contains(_ProductID);
		
		return true;
	}
}
