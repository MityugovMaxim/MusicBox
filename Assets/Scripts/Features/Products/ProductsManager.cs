using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProductsManager : IDataManager
{
	public bool Activated { get; private set; }

	public ProductsCollection  Collection => m_ProductsCollection;
	public ProductsDescriptor  Descriptor => m_ProductsDescriptor;
	public ProfileTransactions Profile    => m_ProfileTransactions;
	public VouchersManager     Vouchers   => m_VouchersManager;
	public StoreProcessor      Store      => m_StoreProcessor;

	[Inject] ProductsCollection    m_ProductsCollection;
	[Inject] ProductsDescriptor    m_ProductsDescriptor;
	[Inject] ProfileTransactions   m_ProfileTransactions;
	[Inject] ProfileCoinsParameter m_CoinsParameter;
	[Inject] StoreProcessor        m_StoreProcessor;
	[Inject] VouchersManager       m_VouchersManager;
	[Inject] MenuProcessor         m_MenuProcessor;

	public async Task<bool> Activate()
	{
		if (Activated)
			return true;
		
		int frame = Time.frameCount;
		
		await Task.WhenAll(
			m_ProductsCollection.Load(),
			m_ProductsDescriptor.Load()
		);
		
		await m_StoreProcessor.Load(GetStoreProducts());
		
		Activated = true;
		
		return frame == Time.frameCount;
	}

	public async Task<RequestState> Purchase(string _ProductID)
	{
		try
		{
			string voucherID = GetVoucherID(_ProductID);
			
			return await m_StoreProcessor.Purchase(_ProductID, voucherID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		return RequestState.Fail;
	}

	public bool ContainsProduct(string _ProductID) => Profile.ContainsProduct(_ProductID);

	public string GetVoucherID(string _ProductID) => m_VouchersManager.GetProductVoucherID(_ProductID);

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

	public StoreProduct[] GetStoreProducts()
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.Select(
				_ProductID => new StoreProduct(
					_ProductID,
					GetAppStoreID(_ProductID),
					GetGooglePlayID(_ProductID)
				)
			)
			.ToArray();
	}

	public string GetAppStoreID(string _ProductID)
	{
		ProductSnapshot snapshot = m_ProductsCollection.GetSnapshot(_ProductID);
		
		return snapshot?.AppStoreID ?? string.Empty;
	}

	public string GetGooglePlayID(string _ProductID)
	{
		ProductSnapshot snapshot = Collection.GetSnapshot(_ProductID);
		
		return snapshot?.GooglePlayID ?? string.Empty;
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

	public string GetPriceSign(string _ProductID) => m_StoreProcessor.GetPrice(_ProductID);

	public string GetPriceCode(string _ProductID) => m_StoreProcessor.GetPrice(_ProductID, false);

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
		
		return productType == ProductType.Coins || !Profile.ContainsProduct(_ProductID);
	}
}
