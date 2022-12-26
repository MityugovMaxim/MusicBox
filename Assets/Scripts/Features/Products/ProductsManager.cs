using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Compression;
using AudioBox.Logging;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProductsManager : IDataManager
{
	public ProductsCollection Collection => m_ProductsCollection;
	public ProductsDescriptor Descriptor => m_ProductsDescriptor;
	public ProfileProducts    Profile    => m_ProfileProducts;
	public VouchersManager    Vouchers   => m_VouchersManager;

	[Inject] ProductsCollection    m_ProductsCollection;
	[Inject] ProductsDescriptor    m_ProductsDescriptor;
	[Inject] ProfileProducts       m_ProfileProducts;
	[Inject] ProfileCoinsParameter m_CoinsParameter;
	[Inject] VouchersManager       m_VouchersManager;
	[Inject] StoreManager          m_StoreManager;
	[Inject] MenuProcessor         m_MenuProcessor;

	public Task<bool> Activate()
	{
		return GroupTask.ProcessAsync(
			this,
			m_ProductsCollection.Load,
			m_ProductsDescriptor.Load,
			m_ProfileProducts.Load,
			m_CoinsParameter.Load,
			m_StoreManager.Activate,
			m_VouchersManager.Activate
		);
	}

	public async Task<RequestState> Purchase(string _ProductID)
	{
		try
		{
			string storeID = GetStoreID(_ProductID);
			
			string voucherID = GetVoucherID(_ProductID);
			
			return await m_StoreManager.Purchase(storeID, _ProductID, voucherID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		return RequestState.Fail;
	}

	public string GetVoucherID(string _ProductID) => m_VouchersManager.GetProductVoucherID(_ProductID);

	public string GetProductID(long _Coins)
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.GreaterMin(m_VouchersManager.GetProductDiscount, _Coins);
	}

	public List<string> GetProductIDs()
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetProductIDs(ProductType _ProductType)
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.Where(IsAvailable)
			.Where(_ProductID => GetType(_ProductID) == _ProductType)
			.ToList();
	}

	public List<string> GetRecommendedProductIDs(int _Count) => GetRecommendedProductIDs(_Count, m_CoinsParameter.Value);

	public List<string> GetRecommendedProductIDs(int _Count, long _Coins)
	{
		List<string> productIDs = GetProductIDs(ProductType.Coins)
			.OrderBy(m_VouchersManager.GetProductDiscount)
			.ToList();
		
		int skip = 0;
		while (skip < productIDs.Count - _Count)
		{
			string productID = productIDs[skip];
			
			long coins = m_VouchersManager.GetProductDiscount(productID);
			
			if (coins >= _Coins)
				break;
			
			skip++;
		}
		
		return productIDs.Skip(skip).Take(_Count).ToList();
	}

	public string GetStoreID(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.StoreID ?? string.Empty;
	}

	public string GetPriceSign(string _ProductID)
	{
		string storeID = GetStoreID(_ProductID);
		
		return m_StoreManager.GetPriceSign(storeID);
	}

	public string GetPriceCode(string _ProductID)
	{
		string storeID = GetStoreID(_ProductID);
		
		return m_StoreManager.GetPriceCode(storeID);
	}

	public string GetImage(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _ProductID) => m_ProductsDescriptor.GetTitle(_ProductID);

	public string GetDescription(string _ProductID) => m_ProductsDescriptor.GetDescription(_ProductID);

	public long GetDiscount(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Coins ?? 0;
	}

	public long GetCoins(string _ProductID) => m_VouchersManager.GetProductDiscount(_ProductID);

	public string GetSeasonID(string _ProductID)
	{
		ProductSnapshot snapshot = Collection.GetSnapshot(_ProductID);
		
		return snapshot?.SeasonID ?? string.Empty;
	}

	public List<string> GetSongIDs(string _ProductID)
	{
		ProductSnapshot snapshot = Collection.GetSnapshot(_ProductID);
		
		return snapshot?.SongIDs ?? new List<string>();
	}

	ProductType GetType(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.Type ?? ProductType.None;
	}

	bool IsActive(string _ProductID)
	{
		ProductSnapshot snapshot = Collection.GetSnapshot(_ProductID);
		
		return snapshot?.Active ?? false;
	}

	bool IsAvailable(string _ProductID)
	{
		ProductType productType = GetType(_ProductID);
		
		return productType == ProductType.Coins || !Profile.Contains(_ProductID);
	}
}
