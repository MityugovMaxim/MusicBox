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
	public bool   Active   { get; }
	public string Image    { get; }
	public string Title    { get; }
	public string Language { get; }
	public string LevelID  { get; }
	public long   Coins    { get; }
	public int    AdsCount { get; }

	public OfferSnapshot(DataSnapshot _Data)
	{
		ID       = _Data.Key;
		Active   = _Data.GetBool("active");
		Image    = _Data.GetString("image");
		Title    = _Data.GetString("title");
		LevelID  = _Data.GetString("level_id");
		Coins    = _Data.GetLong("coins");
		AdsCount = _Data.GetInt("ads_count");
	}
}

public class OffersProcessor
{
	bool Loaded { get; set; }

	readonly SignalBus         m_SignalBus;
	readonly LanguageProcessor m_LanguageProcessor;
	readonly StorageProcessor  m_StorageProcessor;
	readonly MenuProcessor     m_MenuProcessor;

	readonly List<OfferSnapshot> m_OfferSnapshots = new List<OfferSnapshot>();

	DatabaseReference m_OffersData;

	[Inject]
	public OffersProcessor(
		SignalBus         _SignalBus,
		LanguageProcessor _LanguageProcessor,
		StorageProcessor  _StorageProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
		m_StorageProcessor  = _StorageProcessor;
		m_MenuProcessor     = _MenuProcessor;
	}

	public async Task LoadOffers()
	{
		if (m_OffersData == null)
		{
			m_OffersData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("offers");
			m_OffersData.ValueChanged += OnOffersUpdate;
		}
		
		await FetchOffers();
		
		Loaded = true;
	}

	public async Task<bool> CollectOffer(string _OfferID)
	{
		HttpsCallableReference collectOffer = FirebaseFunctions.DefaultInstance.GetHttpsCallable("CollectOffer");
		
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
		}
		else
		{
			UIErrorMenu errorMenu = m_MenuProcessor.GetMenu<UIErrorMenu>();
			if (errorMenu != null)
			{
				errorMenu.Setup(
					"offer_collect_error",
					m_LanguageProcessor.Get("OFFER_COLLECT_ERROR_TITLE"),
					m_LanguageProcessor.Get("OFFER_COLLECT_ERROR_MESSAGE")
				);
			}
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		return success;
	}

	public List<string> GetOfferIDs()
	{
		return m_OfferSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => m_LanguageProcessor.SupportsLanguage(_Snapshot.Language))
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetTitle(string _OfferID)
	{
		OfferSnapshot snapshot = GetOfferSnapshot(_OfferID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get title failed. Snapshot with ID '{0}' is null.", _OfferID);
			return string.Empty;
		}
		
		return snapshot.Title;
	}

	public string GetLevelID(string _OfferID)
	{
		OfferSnapshot snapshot = GetOfferSnapshot(_OfferID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get level ID failed. Snapshot with ID '{0}' is null.", _OfferID);
			return string.Empty;
		}
		
		return snapshot.LevelID;
	}

	public long GetCoins(string _OfferID)
	{
		OfferSnapshot snapshot = GetOfferSnapshot(_OfferID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get coins failed. Snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return snapshot.Coins;
	}

	public int GetAdsCount(string _OfferID)
	{
		OfferSnapshot snapshot = GetOfferSnapshot(_OfferID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get rewarded count failed. Snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return snapshot.AdsCount;
	}

	async void OnOffersUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[OfferProcessor] Updating offers data...");
		
		await FetchOffers();
		
		Debug.Log("[OfferProcessor] Update offers data complete.");
		
		m_SignalBus.Fire<OfferDataUpdateSignal>();
	}

	async Task FetchOffers()
	{
		m_OfferSnapshots.Clear();
		
		DataSnapshot data = await m_OffersData.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (data == null)
		{
			Debug.LogError("[OffersProcessor] Fetch offers failed.");
			return;
		}
		
		m_OfferSnapshots.AddRange(data.Children.Select(_Data => new OfferSnapshot(_Data)));
	}

	OfferSnapshot GetOfferSnapshot(string _OfferID)
	{
		if (m_OfferSnapshots == null || m_OfferSnapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_OfferID))
		{
			Debug.LogError("[OfferProcessor] Get offer snapshot failed. Offer ID is null or empty.");
			return null;
		}
		
		return m_OfferSnapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _OfferID);
	}
}