using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProfileDataUpdateSignal { }

public class ProfileItem
{
	public string ID        { [UsedImplicitly] get; }
	public long   Timestamp { [UsedImplicitly] get; }

	public ProfileItem(string _ID, long _Timestamp)
	{
		ID        = _ID;
		Timestamp = _Timestamp;
	}
}

public class ProfileSnapshot
{
	public long                              Coins        { get; }
	public int                               Level        { get; }
	public int                               Discs        { get; }
	public IReadOnlyList<string>             SongIDs      { get; }
	public IReadOnlyList<string>             OfferIDs     { get; }
	public IReadOnlyList<ProfileTransaction> Transactions { get; }

	public ProfileSnapshot(DataSnapshot _Data)
	{
		Coins        = _Data.GetLong("coins");
		Discs        = _Data.GetInt("discs");
		Level        = _Data.GetInt("level", 1);
		SongIDs      = _Data.GetChildKeys("song_ids");
		OfferIDs     = _Data.GetChildKeys("offer_ids");
		Transactions = _Data.Child("transactions").Children
			.Select(_Transaction => new ProfileTransaction(_Transaction))
			.ToList();
	}
}

public class ProfileTransaction
{
	public string ID            { [UsedImplicitly] get; }
	public string TransactionID { [UsedImplicitly] get; }
	public string ProductID     { [UsedImplicitly] get; }
	public long   Timestamp     { [UsedImplicitly] get; }

	public ProfileTransaction(DataSnapshot _Data)
	{
		ID            = _Data.Key;
		TransactionID = _Data.GetString("transaction_id");
		ProductID     = _Data.GetString("product_id");
		Timestamp     = _Data.GetLong("timestamp");
	}
}

[Preserve]
public class ProfileProcessor
{
	public long Coins => m_Snapshot?.Coins ?? 0;
	public int  Discs => m_Snapshot?.Discs ?? 0;
	public int  Level => m_ProgressProcessor.ClampLevel(m_Snapshot?.Level ?? 1);

	bool Loaded { get; set; }

	[Inject] SignalBus         m_SignalBus;
	[Inject] SocialProcessor   m_SocialProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] ProductsProcessor m_ProductsProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	ProfileSnapshot m_Snapshot;

	DatabaseReference m_ProfileData;

	public async Task Load()
	{
		if (m_ProfileData != null && m_ProfileData.Key != m_SocialProcessor.UserID)
		{
			Debug.LogFormat("[ProfileProcessor] Change user. From: {0} To: {1}.", m_ProfileData.Key, m_SocialProcessor.UserID);
			
			Loaded                     =  false;
			m_ProfileData.ValueChanged -= OnProfileUpdate;
			m_ProfileData              =  null;
		}
		
		if (m_ProfileData == null)
		{
			m_ProfileData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("profiles").Child(m_SocialProcessor.UserID);
			m_ProfileData.ValueChanged += OnProfileUpdate;
		}
		
		await FetchProfile();
		
		Loaded = true;
	}

	public List<string> GetVisibleProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(_ProductID => !HasProduct(_ProductID))
			.OrderByDescending(_ProductID => Mathf.Abs(m_ProductsProcessor.GetDiscount(_ProductID)))
			.ToList();
	}

	public bool HasSong(string _SongID)
	{
		if (m_Snapshot == null || m_Snapshot.SongIDs == null)
			return false;
		
		return m_Snapshot.SongIDs.Contains(_SongID);
	}

	public bool HasOffer(string _OfferID)
	{
		if (m_Snapshot == null || m_Snapshot.OfferIDs == null)
			return false;
		
		return m_Snapshot.OfferIDs.Contains(_OfferID);
	}

	public bool HasProduct(string _ProductID)
	{
		if (m_Snapshot == null || m_Snapshot.Transactions == null)
			return false;
		
		ProductType productType = m_ProductsProcessor.GetType(_ProductID);
		
		if (productType != ProductType.NonConsumable)
			return false;
		
		return m_Snapshot.Transactions.Any(_Transaction => _Transaction.ProductID == _ProductID);
	}

	public bool HasNoAds()
	{
		if (m_Snapshot == null || m_Snapshot.Transactions == null)
			return false;
		
		List<string> productIDs = m_Snapshot.Transactions
			.Select(_Transaction => _Transaction.ProductID)
			.ToList();
		
		return productIDs.Any(m_ProductsProcessor.IsNoAds);
	}

	public async Task<bool> CheckCoins(long _Coins)
	{
		if (Coins >= _Coins)
			return true;
		
		long requiredCoins = _Coins - Coins;
		
		string productID = m_ProductsProcessor.GetCoinsProductID(requiredCoins);
		
		if (string.IsNullOrEmpty(productID))
			return false;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(productID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
		
		return false;
	}

	async void OnProfileUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded || m_ProfileData.Key != m_SocialProcessor.UserID)
			return;
		
		Debug.Log("[ProfileProcessor] Updating profile data...");
		
		await FetchProfile();
		
		Debug.Log("[ProfileProcessor] Update profile data complete.");
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}

	async Task FetchProfile()
	{
		DataSnapshot profileSnapshot = await m_ProfileData.GetValueAsync(15000, 4);
		
		if (profileSnapshot == null)
		{
			Debug.LogError("[ProfileProcessor] Fetch profile failed.");
			return;
		}
		
		m_Snapshot = new ProfileSnapshot(profileSnapshot);
	}
}
