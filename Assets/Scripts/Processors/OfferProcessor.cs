using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using Zenject;

public class OfferDataUpdateSignal { }

public class OfferSnapshot
{
	public long   Coins         { get; }
	public string LevelID       { get; }
	public int    RewardedCount { get; }

	public OfferSnapshot(
		long   _Coins,
		string _LevelID,
		int    _RewardedCount
	)
	{
		Coins         = _Coins;
		LevelID       = _LevelID;
		RewardedCount = _RewardedCount;
	}
}

public class OfferProcessor
{
	public bool Loaded { get; private set; }

	readonly SignalBus              m_SignalBus;
	readonly ProfileProcessor       m_ProfileProcessor;

	readonly List<string>                      m_OfferIDs       = new List<string>();
	readonly Dictionary<string, OfferSnapshot> m_OfferSnapshots = new Dictionary<string, OfferSnapshot>();

	DatabaseReference      m_OffersData;
	HttpsCallableReference m_CompleteOffer;

	[Inject]
	public OfferProcessor(SignalBus _SignalBus, ProfileProcessor _ProfileProcessor)
	{
		m_SignalBus        = _SignalBus;
		m_ProfileProcessor = _ProfileProcessor;
	}

	public async Task LoadOffers()
	{
		if (m_OffersData == null)
			m_OffersData = FirebaseDatabase.DefaultInstance.RootReference.Child("offers");
		
		await FetchOffers();
		
		if (Loaded)
			return;
		
		Loaded = true;
		
		m_OffersData.ValueChanged += OnOffersUpdate;
	}

	public async Task<bool> CompleteOffer(string _OfferID)
	{
		if (m_CompleteOffer == null)
			m_CompleteOffer = FirebaseFunctions.DefaultInstance.GetHttpsCallable("complete_offer");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["offer_id"] = _OfferID;
		
		HttpsCallableResult result = await m_CompleteOffer.CallAsync(data);
		
		data = result.Data as Dictionary<string, object>;
		
		return data.GetBool("success");
	}

	public List<string> GetOfferIDs()
	{
		return m_OfferIDs.SkipWhile(m_ProfileProcessor.HasOffer).ToList();
	}

	public string GetLevelID(string _OfferID)
	{
		OfferSnapshot offerSnapshot = GetOfferSnapshot(_OfferID);
		
		if (offerSnapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get level ID failed. Offer snapshot with ID '{0}' is null.", _OfferID);
			return string.Empty;
		}
		
		return offerSnapshot.LevelID;
	}

	public long GetCoins(string _OfferID)
	{
		OfferSnapshot offerSnapshot = GetOfferSnapshot(_OfferID);
		
		if (offerSnapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get coins failed. Offer snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return offerSnapshot.Coins;
	}

	public int GetRewardedCount(string _OfferID)
	{
		OfferSnapshot offerSnapshot = GetOfferSnapshot(_OfferID);
		
		if (offerSnapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get rewarded count failed. Offer snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return offerSnapshot.RewardedCount;
	}

	async void OnOffersUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[OfferProcessor] Updating offers data...");
		
		await FetchOffers();
		
		Debug.Log("[OfferProcessor] Update offers data complete.");
		
		m_SignalBus.Fire<OfferDataUpdateSignal>();
	}

	async Task FetchOffers()
	{
		DataSnapshot offersSnapshot = await m_OffersData.GetValueAsync();
		
		foreach (DataSnapshot offerSnapshot in offersSnapshot.Children)
		{
			#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
			bool active = offerSnapshot.GetBool("active");
			if (!active)
				continue;
			#endif
			
			string offerID = offerSnapshot.Key;
			
			OfferSnapshot offer = new OfferSnapshot(
				offerSnapshot.GetLong("coins"),
				offerSnapshot.GetString("level_id"),
				offerSnapshot.GetInt("rewarded_count")
			);
			
			m_OfferIDs.Add(offerID);
			m_OfferSnapshots[offerID] = offer;
		}
	}

	OfferSnapshot GetOfferSnapshot(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID))
		{
			Debug.LogError("[OfferProcessor] Get offer snapshot failed. Offer ID is null or empty.");
			return null;
		}
		
		if (!m_OfferSnapshots.ContainsKey(_OfferID))
		{
			Debug.LogErrorFormat("[OfferProcessor] Get offer snapshot failed. Offer with ID '{0}' not found.");
			return null;
		}
		
		return m_OfferSnapshots[_OfferID];
	}
}