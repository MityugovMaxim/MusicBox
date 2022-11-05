using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Database;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

public class ProfileSnapshot
{
	public long                              Coins        { get; }
	public int                               Level        { get; }
	public int                               Discs        { get; }
	public IReadOnlyList<string>             SongIDs      { get; }
	public IReadOnlyList<string>             OfferIDs     { get; }
	public IReadOnlyList<ProfileTransaction> Transactions { get; }
	public IReadOnlyList<ProfileTimer>       Timers       { get; }

	public ProfileSnapshot(DataSnapshot _Data)
	{
		Coins        = _Data.GetLong("coins");
		Discs        = _Data.GetInt("discs");
		Level        = _Data.GetInt("level", 1);
		SongIDs      = _Data.GetChildKeys("song_ids");
		OfferIDs     = _Data.GetChildKeys("offer_ids");
		Transactions = _Data.Child("transactions").Children
			.Select(_Transaction => new ProfileTransaction(_Transaction))
			.ToList();
		Timers = _Data.Child("timers").Children
			.Select(_Timer => new ProfileTimer(_Timer))
			.ToList();
	}
}

public class ProfileTimer
{
	public string ID             { [UsedImplicitly] get; }
	public long   StartTimestamp { [UsedImplicitly] get; }
	public long   EndTimestamp   { [UsedImplicitly] get; }

	readonly Dictionary<string, object> m_Payload;

	public ProfileTimer(DataSnapshot _Data)
	{
		ID             = _Data.Key;
		StartTimestamp = _Data.GetLong("start_time");
		EndTimestamp   = _Data.GetLong("end_time");
		m_Payload      = _Data.Child("payload").GetValue(true) as Dictionary<string, object>;
	}

	public string GetString(string _Key, string _Default = null) => m_Payload.GetString(_Key, _Default);

	public int GetInt(string _Key, int _Default = 0) => m_Payload.GetInt(_Key, _Default);

	public float GetFloat(string _Key, float _Default = 0) => m_Payload.GetFloat(_Key, _Default);

	public double GetDouble(string _Key, double _Default = 0) => m_Payload.GetDouble(_Key, _Default);

	public long GetLong(string _Key, long _Default = 0) => m_Payload.GetLong(_Key, _Default);
}

public class ProfileTransaction
{
	public string ID            { [UsedImplicitly] get; }
	public string TransactionID { [UsedImplicitly] get; }
	public string ProductID     { [UsedImplicitly] get; }
	public long   Timestamp     { [UsedImplicitly] get; }

	public ProfileTransaction(DataSnapshot _Data)
	{
		ID            = _Data.Key;
		TransactionID = _Data.GetString("transaction_id");
		ProductID     = _Data.GetString("product_id");
		Timestamp     = _Data.GetLong("timestamp");
	}
}

[Preserve]
public class ProfileDataUpdateSignal { }
[Preserve]
public class ProfileCoinsUpdateSignal { }
[Preserve]
public class ProfileDiscsUpdateSignal { }
[Preserve]
public class ProfileLevelUpdateSignal { }
[Preserve]
public class ProfileProductsUpdateSignal { }
[Preserve]
public class ProfileSongsUpdateSignal { }

[Preserve]
public class ProfileTimerSignal { }

[Preserve]
public class ProfileProcessor : IInitializable, IDisposable
{
	public long Coins => m_Snapshot?.Coins ?? 0;
	public int  Discs => m_Snapshot?.Discs ?? 0;
	public int  Level => m_Snapshot?.Level ?? 1;

	public IReadOnlyList<string> SongIDs => m_Snapshot?.SongIDs;

	bool Loaded  { get; set; }
	bool Locked  { get; set; }
	bool Pending { get; set; }

	[Inject] SignalBus         m_SignalBus;
	[Inject] SocialProcessor   m_SocialProcessor;
	[Inject] ProductsProcessor m_ProductsProcessor;
	[Inject] StoreProcessor    m_StoreProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	ProfileSnapshot m_Snapshot;

	DatabaseReference m_ProfileData;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<SongsDataUpdateSignal>(OnSongsLibraryUpdate);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<SongsDataUpdateSignal>(OnSongsLibraryUpdate);
	}

	public async Task Load()
	{
		if (m_ProfileData != null && m_ProfileData.Key != m_SocialProcessor.UserID)
		{
			Debug.LogFormat("[ProfileProcessor] Change user. From: {0} To: {1}.", m_ProfileData.Key, m_SocialProcessor.UserID);
			
			Loaded                     =  false;
			m_ProfileData.ValueChanged -= OnUpdate;
			m_ProfileData              =  null;
		}
		
		if (m_ProfileData == null)
		{
			m_ProfileData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("profiles").Child(m_SocialProcessor.UserID);
			m_ProfileData.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public void Lock()
	{
		Locked = true;
	}

	public void Unlock()
	{
		Locked = false;
		
		if (!Pending)
			return;
		
		Pending = false;
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}

	public bool HasSong(string _SongID)
	{
		return m_Snapshot?.SongIDs?.Contains(_SongID) ?? false;
	}

	public bool HasOffer(string _OfferID)
	{
		return m_Snapshot?.OfferIDs?.Contains(_OfferID) ?? false;
	}

	public bool HasProduct(string _ProductID)
	{
		if (m_Snapshot == null || m_Snapshot.Transactions == null)
			return false;
		
		ProductType productType = m_ProductsProcessor.GetType(_ProductID);
		
		switch (productType)
		{
			case ProductType.Consumable:
				return false;
			case ProductType.NonConsumable:
				return m_Snapshot.Transactions.Any(_Transaction => _Transaction.ProductID == _ProductID);
			case ProductType.Subscription:
				return m_StoreProcessor.Subscribed(_ProductID);
			default:
				return false;
		}
	}

	public bool HasNoAds()
	{
		if (m_Snapshot == null || m_Snapshot.Transactions == null)
			return false;
		
		return m_Snapshot.Transactions
			.Select(_Transaction => _Transaction.ProductID)
			.Where(m_ProductsProcessor.IsNoAds)
			.Any(HasProduct);
	}

	public ProfileTimer GetTimer(string _TimerID)
	{
		return m_Snapshot?.Timers?.FirstOrDefault(_Timer => _Timer.ID == _TimerID);
	}

	public void ProcessTimer()
	{
		m_SignalBus.Fire<ProfileTimerSignal>();
	}

	public async Task<bool> CheckCoins(long _Coins)
	{
		if (Coins >= _Coins)
			return true;
		
		long requiredCoins = _Coins - Coins;
		
		string productID = m_ProductsProcessor.GetCoinsProductID(requiredCoins);
		
		if (string.IsNullOrEmpty(productID))
			return false;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(productID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
		
		return false;
	}

	async void OnUpdate(object _Sender, ValueChangedEventArgs _EventArgs)
	{
		if (!Loaded || m_ProfileData.Key != m_SocialProcessor.UserID)
			return;
		
		Debug.Log("[ProfileProcessor] Updating profile data...");
		
		ProfileSnapshot sourceSnapshot = m_Snapshot;
		
		await Fetch();
		
		ProfileSnapshot targetSnapshot = m_Snapshot;
		
		Debug.Log("[ProfileProcessor] Update profile data complete.");
		
		if (Locked)
		{
			Pending = true;
			
			return;
		}
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
		
		if (sourceSnapshot == null || targetSnapshot == null)
			return;
		
		if (sourceSnapshot.Coins != targetSnapshot.Coins)
			m_SignalBus.Fire<ProfileCoinsUpdateSignal>();
		
		if (sourceSnapshot.Discs != targetSnapshot.Discs)
			m_SignalBus.Fire<ProfileDiscsUpdateSignal>();
		
		if (sourceSnapshot.Level != targetSnapshot.Level)
			m_SignalBus.Fire<ProfileLevelUpdateSignal>();
		
		if (sourceSnapshot.SongIDs?.Count != targetSnapshot.SongIDs?.Count)
			m_SignalBus.Fire<ProfileSongsUpdateSignal>();
		
		if (sourceSnapshot.Transactions?.Count != targetSnapshot.Transactions?.Count)
			m_SignalBus.Fire<ProfileProductsUpdateSignal>();
	}

	async Task Fetch()
	{
		DataSnapshot profileSnapshot = await m_ProfileData.GetValueAsync(15000, 4);
		
		if (profileSnapshot == null)
		{
			Debug.LogError("[ProfileProcessor] Fetch profile failed.");
			return;
		}
		
		m_Snapshot = new ProfileSnapshot(profileSnapshot);
	}

	async void OnSongsLibraryUpdate()
	{
		SongLibraryRequest request = new SongLibraryRequest();
		
		await request.SendAsync();
		
		await Fetch();
		
		m_SignalBus.Fire<ProfileDataUpdateSignal>();
	}
}
