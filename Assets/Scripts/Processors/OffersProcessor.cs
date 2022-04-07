using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class OffersDataUpdateSignal { }

public class OfferSnapshot
{
	public string ID          { get; }
	public bool   Active      { get; }
	public string Image       { get; }
	public string Title       { get; }
	public string Description { get; }
	public string SongID      { get; }
	public long   Coins       { get; }
	public int    AdsCount    { get; }
	public long   Timestamp   { get; }
	public int    Order       { get; }

	public OfferSnapshot(DataSnapshot _Data)
	{
		ID          = _Data.Key;
		Active      = _Data.GetBool("active");
		Image       = _Data.GetString("image");
		Title       = _Data.GetString("title");
		Description = _Data.GetString("description");
		SongID      = _Data.GetString("song_id");
		Coins       = _Data.GetLong("coins");
		AdsCount    = _Data.GetInt("ads_count");
		Timestamp   = _Data.GetLong("timestamp");
		Order       = _Data.GetInt("order");
	}
}

[Preserve]
public class OffersProcessor : IInitializable, IDisposable
{
	bool Loaded { get; set; }

	[Inject] SignalBus         m_SignalBus;
	[Inject] LanguageProcessor m_LanguageProcessor;

	readonly List<OfferSnapshot> m_Snapshots = new List<OfferSnapshot>();

	DatabaseReference m_Data;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	public async Task Load()
	{
		if (m_Data == null)
		{
			string path = $"offers/{m_LanguageProcessor.Language}";
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child(path);
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public List<string> GetOfferIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.OrderBy(_Snapshot => _Snapshot.Order)
			.ThenByDescending(_Snapshot => _Snapshot.Timestamp)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetImage(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		return snapshot?.Title ?? string.Empty;
	}

	public string GetDescription(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		return snapshot?.Description ?? string.Empty;
	}

	public string GetSongID(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		return snapshot?.SongID ?? string.Empty;
	}

	public long GetCoins(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		return snapshot?.Coins ?? 0;
	}

	public int GetAdsCount(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[OfferProcessor] Get rewarded count failed. Snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return snapshot.AdsCount;
	}

	async void OnLanguageSelect()
	{
		if (m_Data == null)
			return;
		
		m_Data.ValueChanged -= OnUpdate;
		m_Data              =  null;
		Loaded              =  false;
		
		await Load();
		
		m_SignalBus.Fire<OffersDataUpdateSignal>();
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[OfferProcessor] Updating offers data...");
		
		await Fetch();
		
		Debug.Log("[OfferProcessor] Update offers data complete.");
		
		m_SignalBus.Fire<OffersDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[OffersProcessor] Fetch offers failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new OfferSnapshot(_Data)));
	}

	OfferSnapshot GetSnapshot(string _OfferID)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_OfferID))
		{
			Debug.LogError("[OfferProcessor] Get snapshot failed. Offer ID is null or empty.");
			return null;
		}
		
		OfferSnapshot snapshot = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _OfferID);
		
		if (snapshot == null)
			Debug.LogErrorFormat("[OffersProcessor] Get snapshot failed. Snapshot with ID '{0}' is null.", _OfferID);
		
		return snapshot;
	}
}