using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

public class ProductSnapshot
{
	public string                Key      { get; }
	public string                ID       { get; }
	public bool                  Active   { get; }
	public ProductType           Type     { get; }
	public bool                  Promo    { get; }
	public bool                  Special  { get; }
	public bool                  NoAds    { get; }
	public long                  Coins    { get; }
	public float                 Discount { get; }
	public IReadOnlyList<string> SongIDs  { get; }

	public ProductSnapshot(DataSnapshot _Data)
	{
		Key = _Data.Key;
		#if UNITY_IOS
		ID = _Data.GetString("app_store", _Data.Key);
		#elif UNITY_ANDROID
		ID = _Data.GetString("google_play", _Data.Key);
		#else
		ID = _Data.Key;
		#endif
		
		Active   = _Data.GetBool("active");
		Type     = _Data.GetEnum<ProductType>("type");
		Promo    = _Data.GetBool("promo");
		Special  = _Data.GetBool("special");
		Coins    = _Data.GetLong("coins");
		Discount = _Data.GetFloat("discount");
		NoAds    = _Data.GetBool("no_ads");
		SongIDs  = _Data.GetChildKeys("song_ids");
	}
}

[Preserve]
public class ProductsDataUpdateSignal { }

[Preserve]
public class ProductsDescriptor : DescriptorProcessor<ProductsDataUpdateSignal>
{
	protected override string Path => "products_descriptors";
}

[Preserve]
public class ProductsProcessor
{
	bool Loaded { get; set; }

	[Inject] SignalBus          m_SignalBus;
	[Inject] ProductsDescriptor m_ProductsDescriptor;

	readonly List<ProductSnapshot> m_Snapshots = new List<ProductSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("products");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		await m_ProductsDescriptor.Load();
		
		Loaded = true;
	}

	public List<string> GetProductIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetTitle(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null)
			return string.Empty;
		
		return m_ProductsDescriptor.GetTitle(snapshot.Key);
	}

	public string GetDescription(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null)
			return string.Empty;
		
		return m_ProductsDescriptor.GetDescription(snapshot.Key);
	}

	public ProductType GetType(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Type ?? ProductType.Consumable;
	}

	public long GetCoins(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Coins ?? 0;
	}

	public float GetDiscount(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Discount ?? 0;
	}

	public List<string> GetSongIDs(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.SongIDs != null
			? snapshot.SongIDs.ToList()
			: new List<string>();
	}

	public bool IsPromo(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Promo ?? false;
	}

	public bool IsSpecial(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Special ?? false;
	}

	public bool IsNoAds(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.NoAds ?? false;
	}

	public string GetCoinsProductID(long _Coins)
	{
		ProductSnapshot snapshot = m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Type == ProductType.Consumable)
			.OrderBy(_Snapshot => _Snapshot.Coins)
			.Aggregate((_A, _B) => _A.Coins < _B.Coins && _A.Coins >= _Coins ? _A : _B);
		
		return snapshot?.ID;
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
		
		Log.Info(this, "Updating products data...");
		
		await Fetch();
		
		Log.Info(this, "Update products data complete.");
		
		m_SignalBus.Fire<ProductsDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch products failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new ProductSnapshot(_Data)));
	}

	ProductSnapshot GetSnapshot(string _ProductID)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_ProductID))
		{
			Log.Error(this, "Get product snapshot failed. Product ID is null or empty.");
			return null;
		}
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _ProductID);
	}
}