using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

public class ProductSnapshot
{
	public string                ID       { get; }
	public bool                  Active   { get; }
	public ProductType           Type     { get; }
	public bool                  Promo    { get; }
	public bool                  NoAds    { get; }
	public long                  Coins    { get; }
	public float                 Discount { get; }
	public IReadOnlyList<string> SongIDs { get; }

	public ProductSnapshot(DataSnapshot _Data)
	{
		ID       = _Data.Key;
		Active   = _Data.GetBool("active");
		Type     = _Data.GetEnum<ProductType>("type");
		Promo    = _Data.GetBool("promo");
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

	public string GetTitle(string _ProductID) => m_ProductsDescriptor.GetTitle(_ProductID);

	public string GetDescription(string _ProductID) => m_ProductsDescriptor.GetDescription(_ProductID);

	public ProductType GetType(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get type failed. Product snapshot with ID '{0}' is null.", _ProductID);
			return ProductType.Consumable;
		}
		
		return productSnapshot.Type;
	}

	public long GetCoins(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get coins failed. Product snapshot with ID '{0}' is null.", _ProductID);
			return 0;
		}
		
		return productSnapshot.Coins;
	}

	public float GetDiscount(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[ProductsProcessor] Get discount failed. Snapshot with ID '{0}' is null.", _ProductID);
			return 0;
		}
		
		return snapshot.Discount;
	}

	public List<string> GetSongIDs(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[ProductsProcessor] Get level IDs failed. Snapshot with ID '{0}' is null", _ProductID);
			return new List<string>();
		}
		
		return snapshot.SongIDs != null
			? snapshot.SongIDs.ToList()
			: new List<string>();
	}

	public bool IsPromo(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[ProductsProcessor] Promo check failed. Snapshot with ID '{0}' is null.", _ProductID);
			return false;
		}
		
		return snapshot.Promo;
	}

	public bool IsNoAds(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[ProductsProcessor] No ads check failed. Snapshot with ID '{0}' is null.", _ProductID);
			return false;
		}
		
		return snapshot.NoAds;
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

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[ProductsProcessor] Updating products data...");
		
		await Fetch();
		
		Debug.Log("[ProductsProcessor] Update products data complete.");
		
		m_SignalBus.Fire<ProductsDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[ProductsProcessor] Fetch products failed.");
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
			Debug.LogError("[ProductsProcessor] Get product snapshot failed. Product ID is null or empty.");
			return null;
		}
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _ProductID);
	}
}