using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Functions;
using UnityEngine;
using Zenject;

public class OfferDataUpdateSignal { }

public class OfferSnapshot
{
	public string Title    { get; }
	public string LevelID  { get; }
	public long   Coins    { get; }
	public int    AdsCount { get; }

	public OfferSnapshot(
		string _Title,
		string _LevelID,
		long   _Coins,
		int    _AdsCount
	)
	{
		Title    = _Title;
		LevelID  = _LevelID;
		Coins    = _Coins;
		AdsCount = _AdsCount;
	}
}

public class OffersProcessor
{
	public bool Loaded { get; private set; }

	readonly SignalBus        m_SignalBus;
	readonly ProfileProcessor m_ProfileProcessor;

	readonly List<string>                      m_OfferIDs       = new List<string>();
	readonly Dictionary<string, OfferSnapshot> m_OfferSnapshots = new Dictionary<string, OfferSnapshot>();

	DatabaseReference m_OffersData;

	[Inject]
	public OffersProcessor(
		SignalBus        _SignalBus,
		ProfileProcessor _ProfileProcessor
	)
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

	public async Task<bool> CollectOffer(string _OfferID)
	{
		HttpsCallableReference collectOffer = FirebaseFunctions.DefaultInstance.GetHttpsCallable("collectOffer");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["offer_id"] = _OfferID;
		
		try
		{
			HttpsCallableResult result = await collectOffer.CallAsync(data);
			
			bool success = (bool)result.Data;
			
			await m_ProfileProcessor.LoadProfile();
			
			return success;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public List<string> GetOfferIDs()
	{
		return m_OfferIDs.SkipWhile(IsOfferCollected).ToList();
	}

	public bool IsOfferCollected(string _OfferID)
	{
		return m_ProfileProcessor.Offers != null && m_ProfileProcessor.Offers.Any(_Offer => _Offer.ID == _OfferID);
	}

	public string GetTitle(string _OfferID)
	{
		OfferSnapshot offerSnapshot = GetOfferSnapshot(_OfferID);
		
		if (offerSnapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get title failed. Offer snapshot with ID '{0}' is null.", _OfferID);
			return string.Empty;
		}
		
		return offerSnapshot.Title;
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

	public int GetAdsCount(string _OfferID)
	{
		OfferSnapshot offerSnapshot = GetOfferSnapshot(_OfferID);
		
		if (offerSnapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get rewarded count failed. Offer snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return offerSnapshot.AdsCount;
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
		m_OfferIDs.Clear();
		m_OfferSnapshots.Clear();
		
		DataSnapshot offersSnapshot = await m_OffersData.OrderByChild("order").GetValueAsync();
		
		foreach (DataSnapshot offerSnapshot in offersSnapshot.Children)
		{
			#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
			bool active = offerSnapshot.GetBool("active");
			if (!active)
				continue;
			#endif
			
			string offerID = offerSnapshot.Key;
			
			OfferSnapshot offer = new OfferSnapshot(
				offerSnapshot.GetString("title", string.Empty),
				offerSnapshot.GetString("level_id", string.Empty),
				offerSnapshot.GetLong("coins"),
				offerSnapshot.GetInt("ads_count")
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
			Debug.LogErrorFormat("[OfferProcessor] Get offer snapshot failed. Offer with ID '{0}' not found.", _OfferID);
			return null;
		}
		
		return m_OfferSnapshots[_OfferID];
	}
}