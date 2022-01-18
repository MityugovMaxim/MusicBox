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
	public long                       Coins         { get; }
	public int                        BronzeDiscs   { get; }
	public int                        SilverDiscs   { get; }
	public int                        GoldDiscs     { get; }
	public int                        PlatinumDiscs { get; }
	public IReadOnlyList<ProfileItem> Levels        { get; }
	public IReadOnlyList<ProfileItem> Offers        { get; }
	public IReadOnlyList<ProfileItem> Products      { get; }

	public ProfileSnapshot(DataSnapshot _Data)
	{
		Coins         = _Data.GetLong("coins");
		BronzeDiscs   = _Data.GetInt("bronze_discs");
		SilverDiscs   = _Data.GetInt("silver_discs");
		GoldDiscs     = _Data.GetInt("gold_discs");
		PlatinumDiscs = _Data.GetInt("platinum_discs");
		Levels        = _Data.Child("levels").Children.Select(_Item => new ProfileItem(_Item.Key, _Item.GetLong())).ToList();
		Offers        = _Data.Child("offers").Children.Select(_Item => new ProfileItem(_Item.Key, _Item.GetLong())).ToList();
		Products      = _Data.Child("products").Children.Select(_Item => new ProfileItem(_Item.Key, _Item.GetLong())).ToList();
	}
}

public class ProfileProcessor
{
	public bool                       Loaded        { get; private set; }
	public int                        Discs         => BronzeDiscs + SilverDiscs + GoldDiscs + PlatinumDiscs;
	public int                        BronzeDiscs   => m_ProfileSnapshot?.BronzeDiscs ?? 0;
	public int                        SilverDiscs   => m_ProfileSnapshot?.SilverDiscs ?? 0;
	public int                        GoldDiscs     => m_ProfileSnapshot?.GoldDiscs ?? 0;
	public int                        PlatinumDiscs => m_ProfileSnapshot?.PlatinumDiscs ?? 0;
	public long                       Coins         => m_ProfileSnapshot?.Coins ?? 0;
	public IReadOnlyList<ProfileItem> Levels        => m_ProfileSnapshot?.Levels;
	public IReadOnlyList<ProfileItem> Offers        => m_ProfileSnapshot?.Offers;
	public IReadOnlyList<ProfileItem> Products      => m_ProfileSnapshot?.Products;

	readonly SignalBus         m_SignalBus;
	readonly SocialProcessor   m_SocialProcessor;
	readonly ProgressProcessor m_ProgressProcessor;
	readonly LevelProcessor    m_LevelProcessor;
	readonly StoreProcessor    m_StoreProcessor;
	readonly MenuProcessor     m_MenuProcessor;

	ProfileSnapshot m_ProfileSnapshot;

	DatabaseReference m_ProfileData;

	[Inject]
	public ProfileProcessor(
		SignalBus         _SignalBus,
		SocialProcessor   _SocialProcessor,
		ProgressProcessor _ProgressProcessor,
		LevelProcessor    _LevelProcessor,
		StoreProcessor    _StoreProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_SocialProcessor   = _SocialProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_StoreProcessor    = _StoreProcessor;
		m_MenuProcessor     = _MenuProcessor;
	}

	public async Task LoadProfile()
	{
		if (m_ProfileData == null)
			m_ProfileData = FirebaseDatabase.DefaultInstance.RootReference.Child("profiles").Child(m_SocialProcessor.UserID);
		
		await FetchProfile();
		
		if (Loaded)
			return;
		
		Loaded = true;
		
		m_ProfileData.ValueChanged += OnProfileUpdate;
	}

	public int GetLevel()
	{
		return m_ProgressProcessor.GetLevel(Discs);
	}

	public float GetProgress()
	{
		return m_ProgressProcessor.GetProgress(Discs);
	}

	public bool IsLevelLocked(string _LevelID)
	{
		if (!m_StoreProcessor.IsLevelPurchased(_LevelID))
			return true;
		
		int requiredLevel = m_LevelProcessor.GetLevel(_LevelID);
		int currentLevel  = GetLevel();
		
		if (requiredLevel > currentLevel)
			return true;
		
		long price = m_LevelProcessor.GetPrice(_LevelID);
		
		if (price == 0)
			return false;
		
		return Levels != null && Levels.All(_Level => _Level.ID != _LevelID);
	}

	public bool IsLevelUnlocked(string _LevelID)
	{
		return !IsLevelLocked(_LevelID);
	}

	public async Task<bool> CompleteLevel(string _LevelID, ScoreRank _Rank, int _Accuracy, long _Score)
	{
		HttpsCallableReference completeLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("completeLevel");
		
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
			HttpsCallableResult result = await unlockLevel.CallAsync(MiniJson.JsonEncode(data));
			
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
		
		string productID = m_StoreProcessor.GetProductIDs()
			.Where(_ProductID => m_StoreProcessor.GetCoins(_ProductID) >= requiredCoins)
			.Aggregate((_A, _B) => m_StoreProcessor.GetCoins(_A) < m_StoreProcessor.GetCoins(_B) ? _A : _B);
		
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
		Debug.Log("[ProfileProcessor] Updating profile data...");
		
		await FetchProfile();
		
		Debug.Log("[ProfileProcessor] Update profile data complete.");
	}

	async Task FetchProfile()
	{
		DataSnapshot profileSnapshot = await m_ProfileData.GetValueAsync();
		
		m_ProfileSnapshot = new ProfileSnapshot(profileSnapshot);
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}
}
