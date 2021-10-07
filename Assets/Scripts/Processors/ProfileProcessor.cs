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

public class ProfileSnapshot
{
	public int                   Level         { get; }
	public long                  Coins         { get; }
	public int                   BronzeDiscs   { get; }
	public int                   SilverDiscs   { get; }
	public int                   GoldDiscs     { get; }
	public int                   PlatinumDiscs { get; }
	public IReadOnlyList<string> LevelIDs      { get; }
	public IReadOnlyList<string> OfferIDs      { get; }
	public IReadOnlyList<string> ProductIDs    { get; }

	public ProfileSnapshot(
		int                   _Level,
		long                  _Coins,
		int                   _BronzeDiscs,
		int                   _SilverDiscs,
		int                   _GoldDiscs,
		int                   _PlatinumDiscs,
		IReadOnlyList<string> _LevelIDs,
		IReadOnlyList<string> _OfferIDs,
		IReadOnlyList<string> _ProductIDs
	)
	{
		Level         = _Level;
		Coins         = _Coins;
		BronzeDiscs   = _BronzeDiscs;
		SilverDiscs   = _SilverDiscs;
		GoldDiscs     = _GoldDiscs;
		PlatinumDiscs = _PlatinumDiscs;
		LevelIDs      = _LevelIDs;
		OfferIDs      = _OfferIDs;
		ProductIDs    = _ProductIDs;
	}
}

public class ProgressSnapshot
{
	public long                      BronzeDiscExp   { get; }
	public long                      SilverDiscExp   { get; }
	public long                      GoldDiscExp     { get; }
	public long                      PlatinumDiscExp { get; }
	public IReadOnlyCollection<long> Levels          { get; }

	public ProgressSnapshot(
		long                      _BronzeDiscExp,
		long                      _SilverDiscExp,
		long                      _GoldDiscExp,
		long                      _PlatinumDiscExp,
		IReadOnlyCollection<long> _Levels
	)
	{
		BronzeDiscExp   = _BronzeDiscExp;
		SilverDiscExp   = _SilverDiscExp;
		GoldDiscExp     = _GoldDiscExp;
		PlatinumDiscExp = _PlatinumDiscExp;
		Levels          = _Levels;
	}
}

public class ProfileProcessor : IInitializable, IDisposable
{
	public int                         Level         => m_ProfileSnapshot?.Level ?? 0;
	public int                         BronzeDiscs   => m_ProfileSnapshot?.BronzeDiscs ?? 0;
	public int                         SilverDiscs   => m_ProfileSnapshot?.SilverDiscs ?? 0;
	public int                         GoldDiscs     => m_ProfileSnapshot?.GoldDiscs ?? 0;
	public int                         PlatinumDiscs => m_ProfileSnapshot?.PlatinumDiscs ?? 0;
	public long                        Coins         => m_ProfileSnapshot?.Coins ?? 0;
	public IReadOnlyCollection<string> LevelIDs      => m_ProfileSnapshot?.LevelIDs;

	readonly SignalBus         m_SignalBus;
	readonly SocialProcessor   m_SocialProcessor;
	readonly LevelProcessor    m_LevelProcessor;
	readonly StoreProcessor m_StoreProcessor;

	ProfileSnapshot  m_ProfileSnapshot;
	ProgressSnapshot m_ProgressSnapshot;

	DatabaseReference      m_ProfileData;
	DatabaseReference      m_LevelData;
	HttpsCallableReference m_CompleteLevel;
	HttpsCallableReference m_UnlockLevel;

	[Inject]
	public ProfileProcessor(
		SignalBus         _SignalBus,
		SocialProcessor   _SocialProcessor,
		LevelProcessor    _LevelProcessor,
		StoreProcessor _StoreProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LevelProcessor    = _LevelProcessor;
		m_SocialProcessor   = _SocialProcessor;
		m_StoreProcessor = _StoreProcessor;
	}

	public async Task LoadProfile()
	{
		if (m_ProfileData == null)
			m_ProfileData = FirebaseDatabase.DefaultInstance.RootReference.Child("profiles").Child(m_SocialProcessor.UserID);
		
		await FetchProfile();
		
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

	public long GetExp()
	{
		if (m_ProfileSnapshot == null || m_ProgressSnapshot == null)
			return 0;
		
		long exp = 0;
		exp += m_ProfileSnapshot.BronzeDiscs * m_ProgressSnapshot.BronzeDiscExp;
		exp += m_ProfileSnapshot.SilverDiscs * m_ProgressSnapshot.SilverDiscExp;
		exp += m_ProfileSnapshot.GoldDiscs * m_ProgressSnapshot.GoldDiscExp;
		exp += m_ProfileSnapshot.PlatinumDiscs * m_ProgressSnapshot.PlatinumDiscExp;
		return exp;
	}

	public float GetExpProgress()
	{
		long exp    = GetExp();
		long source = m_ProgressSnapshot.Levels.Where(_Exp => _Exp <= exp).Min();
		long target = m_ProgressSnapshot.Levels.Where(_Exp => _Exp >= exp).Min();
		
		return MathUtility.Remap01Clamped(exp, source, target);
	}

	public long GetPayout(string _LevelID)
	{
		return m_LevelProcessor.GetPayout(_LevelID);
	}

	public long GetPayout(string _LevelID, ScoreRank _Rank)
	{
		return GetPayout(_LevelID) * GetPayoutMultiplier(_Rank);
	}

	public long GetPrice(string _LevelID)
	{
		return m_LevelProcessor.GetPrice(_LevelID);
	}

	public bool HasOffer(string _OfferID)
	{
		return m_ProfileSnapshot?.OfferIDs.Contains(_OfferID) ?? false;
	}

	public bool HasLevel(string _LevelID)
	{
		return m_ProfileSnapshot?.LevelIDs.Contains(_LevelID) ?? false;
	}

	public bool HasProduct(string _ProductID)
	{
		return m_ProfileSnapshot?.ProductIDs.Contains(_ProductID) ?? false;
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
		
		return LevelIDs != null && !LevelIDs.Contains(_LevelID);
	}

	public bool IsLevelUnlocked(string _LevelID)
	{
		return !IsLevelLocked(_LevelID);
	}

	public async Task UnlockLevel(string _LevelID)
	{
		long price = GetPrice(_LevelID);
		
		if (m_ProfileSnapshot != null && price > Coins)
		{
			// TODO: Open shop
			Debug.LogErrorFormat("[ProgressProcessor] Unlock level failed. Not enough coins. Required: {0}. Current: {1}.", price, Coins);
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
			profileSnapshot.GetChildKeys("levels"),
			profileSnapshot.GetChildKeys("offers"),
			profileSnapshot.GetChildKeys("products")
		);
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}

	static int GetPayoutMultiplier(ScoreRank _Rank)
	{
		switch (_Rank)
		{
			case ScoreRank.Platinum:
				return 3;
			case ScoreRank.Gold:
				return 2;
			case ScoreRank.Silver:
				return 1;
			case ScoreRank.Bronze:
				return 1;
			default:
				return 0;
		}
	}
}
