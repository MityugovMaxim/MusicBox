using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;
using Zenject;

public class ProfileDataUpdateSignal { }

public class ProfileItem
{
	public string ID        { get; }
	public long   Timestamp { get; }

	public ProfileItem(string _ID, long _Timestamp)
	{
		ID        = _ID;
		Timestamp = _Timestamp;
	}
}

public class ProfileSnapshot
{
	public int                        Level         { get; }
	public long                       Coins         { get; }
	public int                        BronzeDiscs   { get; }
	public int                        SilverDiscs   { get; }
	public int                        GoldDiscs     { get; }
	public int                        PlatinumDiscs { get; }
	public IReadOnlyList<ProfileItem> Levels        { get; }
	public IReadOnlyList<ProfileItem> Offers        { get; }
	public IReadOnlyList<ProfileItem> Products      { get; }

	public ProfileSnapshot(
		int                        _Level,
		long                       _Coins,
		int                        _BronzeDiscs,
		int                        _SilverDiscs,
		int                        _GoldDiscs,
		int                        _PlatinumDiscs,
		IReadOnlyList<ProfileItem> _Levels,
		IReadOnlyList<ProfileItem> _Offers,
		IReadOnlyList<ProfileItem> _Products
	)
	{
		Level         = _Level;
		Coins         = _Coins;
		BronzeDiscs   = _BronzeDiscs;
		SilverDiscs   = _SilverDiscs;
		GoldDiscs     = _GoldDiscs;
		PlatinumDiscs = _PlatinumDiscs;
		Levels        = _Levels;
		Offers        = _Offers;
		Products      = _Products;
	}
}

public class ProfileProcessor : IInitializable, IDisposable
{
	public bool Loaded { get; private set; }

	public int                        Level         => m_ProfileSnapshot?.Level ?? 0;
	public int                        BronzeDiscs   => m_ProfileSnapshot?.BronzeDiscs ?? 0;
	public int                        SilverDiscs   => m_ProfileSnapshot?.SilverDiscs ?? 0;
	public int                        GoldDiscs     => m_ProfileSnapshot?.GoldDiscs ?? 0;
	public int                        PlatinumDiscs => m_ProfileSnapshot?.PlatinumDiscs ?? 0;
	public long                       Coins         => m_ProfileSnapshot?.Coins ?? 0;
	public IReadOnlyList<ProfileItem> Levels        => m_ProfileSnapshot?.Levels;
	public IReadOnlyList<ProfileItem> Offers        => m_ProfileSnapshot?.Offers;
	public IReadOnlyList<ProfileItem> Products      => m_ProfileSnapshot?.Products;

	readonly SignalBus       m_SignalBus;
	readonly SocialProcessor m_SocialProcessor;
	readonly LevelProcessor  m_LevelProcessor;
	readonly StoreProcessor  m_StoreProcessor;
	readonly MenuProcessor   m_MenuProcessor;

	ProfileSnapshot  m_ProfileSnapshot;

	DatabaseReference      m_ProfileData;
	DatabaseReference      m_LevelData;
	HttpsCallableReference m_CompleteLevel;
	HttpsCallableReference m_UnlockLevel;

	[Inject]
	public ProfileProcessor(
		SignalBus       _SignalBus,
		SocialProcessor _SocialProcessor,
		LevelProcessor  _LevelProcessor,
		StoreProcessor  _StoreProcessor,
		MenuProcessor   _MenuProcessor
	)
	{
		m_SignalBus       = _SignalBus;
		m_LevelProcessor  = _LevelProcessor;
		m_SocialProcessor = _SocialProcessor;
		m_StoreProcessor  = _StoreProcessor;
		m_MenuProcessor   = _MenuProcessor;
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

	async void OnProfileUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[ProgressProcessor] Updating profile data...");
		
		await FetchProfile();
		
		Debug.Log("[ProgressProcessor] Update profile data complete.");
	}

	void IInitializable.Initialize()
	{
		m_CompleteLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("complete_level");
		m_UnlockLevel   = FirebaseFunctions.DefaultInstance.GetHttpsCallable("unlock_level");
		
		m_SignalBus.Subscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	async void RegisterLevelFinish(LevelFinishSignal _Signal)
	{
		HttpsCallableResult result = await m_CompleteLevel.CallAsync();
		
		Dictionary<string, object> data = MiniJson.JsonDecode((string)result.Data) as Dictionary<string, object>;
		
		if (data == null || !data.GetBool("success"))
			return;
		
		await FetchProfile();
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}

	public bool IsLevelLocked(string _LevelID)
	{
		if (!m_StoreProcessor.IsLevelPurchased(_LevelID))
			return true;
		
		int level = m_LevelProcessor.GetLevel(_LevelID);
		
		if (level > Level)
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

	public async Task UnlockLevel(string _LevelID)
	{
		long price = m_LevelProcessor.GetPrice(_LevelID);
		
		if (m_ProfileSnapshot != null && price > Coins)
		{
			Debug.LogWarningFormat("[ProgressProcessor] Unlock level failed. Not enough coins. Required: {0}. Current: {1}.", price, Coins);
			
			string productID = m_StoreProcessor.GetProductIDs()
				.SkipWhile(m_StoreProcessor.IsProductPurchased)
				.SkipWhile(_ProductID => m_StoreProcessor.GetCoins(_ProductID) < price)
				.Aggregate((_A, _B) => m_StoreProcessor.GetCoins(_A) < m_StoreProcessor.GetCoins(_B) ? _A : _B);
			
			if (string.IsNullOrEmpty(productID))
				return;
			
			UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
			
			UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
			
			if (productMenu != null)
				productMenu.Setup(productID);
			
			await m_MenuProcessor.Show(MenuType.ProductMenu);
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			
			if (mainMenu != null)
				mainMenu.Select(MainMenuPageType.Store, true);
			
			return;
		}
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["level_id"] = _LevelID;
		
		HttpsCallableResult result = await m_UnlockLevel.CallAsync(MiniJson.JsonEncode(data));
		
		data = MiniJson.JsonDecode((string)result.Data) as Dictionary<string, object>;
		
		if (data == null || !data.GetBool("success"))
			return;
		
		await FetchProfile();
	}

	async Task FetchProfile()
	{
		DataSnapshot profileSnapshot = await m_ProfileData.GetValueAsync();
		
		m_ProfileSnapshot = new ProfileSnapshot(
			profileSnapshot.GetInt("level"),
			profileSnapshot.GetLong("coins"),
			profileSnapshot.GetInt("bronze_discs"),
			profileSnapshot.GetInt("silver_discs"),
			profileSnapshot.GetInt("gold_discs"),
			profileSnapshot.GetInt("platinum_discs"),
			profileSnapshot.Child("levels").Children.Select(_Item => new ProfileItem(_Item.Key, _Item.GetLong())).ToList(),
			profileSnapshot.Child("offers").Children.Select(_Item => new ProfileItem(_Item.Key, _Item.GetLong())).ToList(),
			profileSnapshot.Child("products").Children.Select(_Item => new ProfileItem(_Item.Key, _Item.GetLong())).ToList()
		);
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}
}
