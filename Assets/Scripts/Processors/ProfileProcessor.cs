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
	public string ID        { get; }
	public long   Timestamp { [UsedImplicitly] get; }

	public ProfileItem(string _ID, long _Timestamp)
	{
		ID        = _ID;
		Timestamp = _Timestamp;
	}
}

public class ProfileSnapshot
{
	public long                  Coins    { get; }
	public int                   Level    { get; }
	public int                   Discs    { get; }
	public IReadOnlyList<string> SongIDs   { get; }
	public IReadOnlyList<string> OfferIDs   { get; }
	public IReadOnlyList<string> ProductsIDs { get; }

	public ProfileSnapshot(DataSnapshot _Data)
	{
		Coins       = _Data.GetLong("coins");
		Discs       = _Data.GetInt("discs");
		Level       = _Data.GetInt("level", 1);
		SongIDs    = _Data.GetChildKeys("levels");
		OfferIDs    = _Data.GetChildKeys("offers");
		ProductsIDs = _Data.GetChildKeys("products");
	}
}

public class ProfileProcessor
{
	public int                   Level      => m_ProgressProcessor.ClampLevel(m_ProfileSnapshot?.Level ?? 1);
	public int                   Discs      => m_ProfileSnapshot?.Discs ?? 0;
	public long                  Coins      => m_ProfileSnapshot?.Coins ?? 0;
	public IReadOnlyList<string> SongIDs    => m_ProfileSnapshot?.SongIDs;
	public IReadOnlyList<string> OfferIDs   => m_ProfileSnapshot?.OfferIDs;
	public IReadOnlyList<string> ProductIDs => m_ProfileSnapshot?.ProductsIDs;

	bool Loaded { get; set; }

	[Inject] SignalBus         m_SignalBus;
	[Inject] SocialProcessor   m_SocialProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] ProductsProcessor m_ProductsProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	ProfileSnapshot m_ProfileSnapshot;

	DatabaseReference m_ProfileData;

	public async Task LoadProfile()
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

	public float GetProgress()
	{
		return m_ProgressProcessor.GetProgress(Discs);
	}

	public List<string> GetVisibleProductIDs()
	{
		return m_ProductsProcessor.GetProductIDs()
			.Where(_ProductID => !HasProduct(_ProductID))
			.OrderByDescending(_ProductID => Mathf.Abs(m_ProductsProcessor.GetDiscount(_ProductID)))
			.ToList();
	}

	public bool HasSong(string _LevelID)
	{
		return SongIDs.Contains(_LevelID);
	}

	public bool HasOffer(string _OfferID)
	{
		return OfferIDs.Contains(_OfferID);
	}

	public bool HasProduct(string _ProductID)
	{
		return m_ProductsProcessor.GetType(_ProductID) == ProductType.NonConsumable && ProductIDs.Contains(_ProductID);
	}

	public bool HasNoAds()
	{
		return ProductIDs.Any(m_ProductsProcessor.IsNoAds);
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
		
		m_ProfileSnapshot = new ProfileSnapshot(profileSnapshot);
	}
}
