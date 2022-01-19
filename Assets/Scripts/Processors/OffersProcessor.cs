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
	public string ID       { get; }
	public string Title    { get; }
	public string LevelID  { get; }
	public long   Coins    { get; }
	public int    AdsCount { get; }

	public OfferSnapshot(DataSnapshot _Data)
	{
		ID       = _Data.Key;
		Title    = _Data.GetString("title");
		LevelID  = _Data.GetString("level_id");
		Coins    = _Data.GetLong("coins");
		AdsCount = _Data.GetInt("ads_count");
	}
}

public class OffersProcessor
{
	public bool Loaded { get; private set; }

	readonly SignalBus         m_SignalBus;
	readonly LanguageProcessor m_LanguageProcessor;
	readonly StorageProcessor  m_StorageProcessor;
	readonly ProfileProcessor  m_ProfileProcessor;
	readonly MenuProcessor     m_MenuProcessor;

	readonly List<string>                      m_OfferIDs       = new List<string>();
	readonly Dictionary<string, OfferSnapshot> m_OfferSnapshots = new Dictionary<string, OfferSnapshot>();

	DatabaseReference m_OffersData;

	[Inject]
	public OffersProcessor(
		SignalBus         _SignalBus,
		LanguageProcessor _LanguageProcessor,
		StorageProcessor  _StorageProcessor,
		ProfileProcessor  _ProfileProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
		m_StorageProcessor  = _StorageProcessor;
		m_ProfileProcessor  = _ProfileProcessor;
		m_MenuProcessor     = _MenuProcessor;
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
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await collectOffer.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception)
		{
			success = false;
		}
		
		if (success)
		{
			await Task.Delay(250);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			
			await DisplayReward(_OfferID);
		}
		else
		{
			UIErrorMenu errorMenu = m_MenuProcessor.GetMenu<UIErrorMenu>();
			if (errorMenu != null)
			{
				errorMenu.Setup(
					m_LanguageProcessor.Get("OFFER_COLLECT_ERROR_TITLE"),
					m_LanguageProcessor.Get("OFFER_COLLECT_ERROR_MESSAGE")
				);
			}
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		return success;
	}

	async Task DisplayReward(string _OfferID)
	{
		UIRewardMenu rewardMenu = m_MenuProcessor.GetMenu<UIRewardMenu>();
		
		if (rewardMenu == null)
			return;
		
		rewardMenu.Setup(
			null,
			m_StorageProcessor.LoadOfferThumbnail(_OfferID),
			GetTitle(_OfferID),
			string.Empty
		);
		
		await m_MenuProcessor.Show(MenuType.RewardMenu);
		
		await Task.Delay(2500);
		
		await rewardMenu.Play();
		
		await m_MenuProcessor.Hide(MenuType.RewardMenu);
	}

	public List<string> GetOfferIDs()
	{
		return m_OfferIDs.Where(_OfferID => m_ProfileProcessor.Offers == null || m_ProfileProcessor.Offers.All(_Offer => _Offer.ID != _OfferID)).ToList();
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
			bool active = offerSnapshot.GetBool("active");
			
			if (!active)
				continue;
			
			OfferSnapshot offer = new OfferSnapshot(offerSnapshot);
			
			m_OfferIDs.Add(offer.ID);
			m_OfferSnapshots[offer.ID] = offer;
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