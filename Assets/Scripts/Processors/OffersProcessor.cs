using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public class OfferSnapshot
{
	public string ID          { get; }
	public bool   Active      { get; }
	public string SongID      { get; }
	public long   Coins       { get; }
	public int    AdsCount    { get; }
	public long   Timestamp   { get; }
	public int    Order       { get; }

	public OfferSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		SongID    = _Data.GetString("song_id");
		Coins     = _Data.GetLong("coins");
		AdsCount  = _Data.GetInt("ads_count");
		Timestamp = _Data.GetLong("timestamp");
		Order     = _Data.GetInt("order");
	}
}

[Preserve]
public class OffersDataUpdateSignal { }

[Preserve]
public class OffersDescriptor : DescriptorProcessor<OffersDataUpdateSignal>
{
	protected override string Path => "offers_descriptors";
}

[Preserve]
public class OffersProcessor
{
	bool Loaded { get; set; }

	[Inject] SignalBus        m_SignalBus;
	[Inject] OffersDescriptor m_OffersDescriptor;

	readonly List<OfferSnapshot> m_Snapshots = new List<OfferSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child($"offers");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		await m_OffersDescriptor.Load();
		
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

	public string GetTitle(string _OfferID) => m_OffersDescriptor.GetTitle(_OfferID);

	public string GetDescription(string _OfferID) => m_OffersDescriptor.GetDescription(_OfferID);

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
			Log.Error(this, "Get rewarded count failed. Snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return snapshot.AdsCount;
	}

	void Unload()
	{
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (FirebaseAuth.DefaultInstance.CurrentUser == null)
		{
			Unload();
			return;
		}
		
		Log.Info(this, "Updating offers data...");
		
		await Fetch();
		
		Log.Info(this, "Update offers data complete.");
		
		m_SignalBus.Fire<OffersDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch offers failed.");
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
			Log.Error(this, "Get snapshot failed. Offer ID is null or empty.");
			return null;
		}
		
		OfferSnapshot snapshot = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _OfferID);
		
		if (snapshot == null)
			Log.Error(this, "Get snapshot failed. Snapshot with ID '{0}' is null.", _OfferID);
		
		return snapshot;
	}
}