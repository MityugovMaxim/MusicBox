using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Functions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

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
	public IReadOnlyList<string> LevelIDs   { get; }
	public IReadOnlyList<string> OfferIDs   { get; }
	public IReadOnlyList<string> ProductsIDs { get; }

	public ProfileSnapshot(DataSnapshot _Data)
	{
		Coins       = _Data.GetLong("coins");
		Discs       = _Data.GetInt("discs");
		Level       = _Data.GetInt("level", 1);
		LevelIDs    = _Data.GetChildKeys("levels");
		OfferIDs    = _Data.GetChildKeys("offers");
		ProductsIDs = _Data.GetChildKeys("products");
	}
}

public class ProfileProcessor
{
	public int                   Level      => m_ProfileSnapshot?.Level ?? 1;
	public int                   Discs      => m_ProfileSnapshot?.Discs ?? 0;
	public long                  Coins      => m_ProfileSnapshot?.Coins ?? 0;
	public IReadOnlyList<string> LevelIDs   => m_ProfileSnapshot?.LevelIDs;
	public IReadOnlyList<string> OfferIDs   => m_ProfileSnapshot?.OfferIDs;
	public IReadOnlyList<string> ProductIDs => m_ProfileSnapshot?.ProductsIDs;

	bool Loaded { get; set; }

	readonly SignalBus         m_SignalBus;
	readonly SocialProcessor   m_SocialProcessor;
	readonly ProgressProcessor m_ProgressProcessor;
	readonly LevelProcessor    m_LevelProcessor;
	readonly ProductProcessor  m_ProductProcessor;
	readonly OffersProcessor   m_OffersProcessor;
	readonly ScoreProcessor    m_ScoreProcessor;
	readonly MenuProcessor     m_MenuProcessor;

	ProfileSnapshot m_ProfileSnapshot;

	DatabaseReference m_ProfileData;

	[Inject]
	public ProfileProcessor(
		SignalBus         _SignalBus,
		SocialProcessor   _SocialProcessor,
		ProgressProcessor _ProgressProcessor,
		LevelProcessor    _LevelProcessor,
		ProductProcessor  _ProductProcessor,
		OffersProcessor   _OffersProcessor,
		ScoreProcessor    _ScoreProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_SocialProcessor   = _SocialProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_ProductProcessor  = _ProductProcessor;
		m_OffersProcessor   = _OffersProcessor;
		m_ScoreProcessor    = _ScoreProcessor;
		m_MenuProcessor     = _MenuProcessor;
	}

	public async Task LoadProfile()
	{
		if (m_ProfileData != null && m_ProfileData.Key != m_SocialProcessor.UserID)
		{
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

	// TODO: Region filter
	public List<string> GetVisibleLevelIDs()
	{
		return m_LevelProcessor.GetLevelIDs()
			.OrderByDescending(m_LevelProcessor.GetBadge)
			.ThenBy(m_ScoreProcessor.GetRank)
			.ThenByDescending(m_LevelProcessor.GetLevel)
			.ThenBy(m_LevelProcessor.GetPrice)
			.ToList();
	}

	public List<string> GetVisibleProductIDs()
	{
		return m_ProductProcessor.GetProductIDs()
			.Where(_ProductID => !HasProduct(_ProductID))
			.OrderByDescending(m_ProductProcessor.GetDiscount)
			.ToList();
	}

	public List<string> GetVisibleOfferIDs()
	{
		return m_OffersProcessor.GetOfferIDs()
			.Where(_OfferID => !HasOffer(_OfferID))
			.Distinct()
			.ToList();
	}

	public bool HasLevel(string _LevelID)
	{
		return LevelIDs.Contains(_LevelID) || Level >= m_LevelProcessor.GetLevel(_LevelID);
	}

	public bool IsLevelUnlocked(string _LevelID)
	{
		return LevelIDs.Contains(_LevelID) || Level >= m_LevelProcessor.GetLevel(_LevelID) && m_LevelProcessor.GetPrice(_LevelID) == 0;
	}

	public bool HasOffer(string _OfferID)
	{
		return OfferIDs.Contains(_OfferID);
	}

	public bool HasProduct(string _ProductID)
	{
		return m_ProductProcessor.GetType(_ProductID) == ProductType.NonConsumable && ProductIDs.Contains(_ProductID);
	}

	public bool HasNoAds()
	{
		return ProductIDs.Any(m_ProductProcessor.IsNoAds);
	}

	public async Task<bool> FinishLevel(string _LevelID, ScoreRank _Rank, int _Accuracy, long _Score)
	{
		HttpsCallableReference completeLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("FinishLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"] = _LevelID;
		data["rank"]     = (int)_Rank;
		data["accuracy"] = _Accuracy;
		data["score"]    = _Score;
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await completeLevel.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			
			success = false;
		}
		
		await FetchProfile();
		
		return success;
	}

	public async Task<bool> CompleteLevel(string _LevelID, ScoreRank _Rank, long _Score, int _Accuracy)
	{
		HttpsCallableReference completeLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("CompleteLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"] = _LevelID;
		data["rank"]     = (int)_Rank;
		data["score"]    = _Score;
		data["accuracy"] = _Accuracy;
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await completeLevel.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			
			success = false;
		}
		
		await Task.WhenAll(
			FetchProfile(),
			m_ScoreProcessor.LoadScores()
		);
		
		return success;
	}

	public async Task<bool> UnlockLevel(string _LevelID)
	{
		long coins = m_LevelProcessor.GetPrice(_LevelID);
		
		if (!await CheckCoins(coins))
			return false;
		
		HttpsCallableReference unlockLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("unlockLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"] = _LevelID;
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await unlockLevel.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			
			success = false;
		}
		
		await FetchProfile();
		
		return success;
	}

	public async Task<bool> ReviveLevel(string _LevelID, int _ReviveCount)
	{
		long coins = m_LevelProcessor.GetRevivePrice(_LevelID);
		
		if (!await CheckCoins(coins))
			return false;
		
		HttpsCallableReference revive = FirebaseFunctions.DefaultInstance.GetHttpsCallable("reviveLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"]     = _LevelID;
		data["revive_count"] = _ReviveCount;
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await revive.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			
			success = false;
		}
		
		return success;
	}

	public async Task<bool> CheckCoins(long _Coins)
	{
		if (Coins >= _Coins)
			return true;
		
		long requiredCoins = _Coins - Coins;
		
		string productID = GetVisibleProductIDs()
			.Where(_ProductID => m_ProductProcessor.GetCoins(_ProductID) >= requiredCoins)
			.Aggregate((_A, _B) => m_ProductProcessor.GetCoins(_A) < m_ProductProcessor.GetCoins(_B) ? _A : _B);
		
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
		if (!Loaded)
			return;
		
		Debug.Log("[ProfileProcessor] Updating profile data...");
		
		await FetchProfile();
		
		Debug.Log("[ProfileProcessor] Update profile data complete.");
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}

	async Task FetchProfile()
	{
		DataSnapshot profileSnapshot = await m_ProfileData.GetValueAsync(15000, 2);
		
		if (profileSnapshot == null)
		{
			Debug.LogError("[ProfileProcessor] Fetch profile failed.");
			return;
		}
		
		m_ProfileSnapshot = new ProfileSnapshot(profileSnapshot);
	}
}
