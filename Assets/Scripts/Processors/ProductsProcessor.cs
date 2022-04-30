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
	[ClipboardProperty]
	public string       ID       { get; set; }
	public bool         Active   { get; set; }
	public ProductType  Type     { get; set; }
	public bool         Promo    { get; set; }
	public bool         NoAds    { get; set; }
	public long         Coins    { get; set; }
	public float        Discount { get; set; }
	public List<string> SongIDs  { get; set; }
	[HideProperty]
	public int          Order    { get; set; }

	public ProductSnapshot(string _ProductID)
	{
		ID      = _ProductID;
		SongIDs = new List<string>();
	}

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
		Order    = _Data.GetInt("order");
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]   = Active;
		data["type"]     = (int)Type;
		data["promo"]    = Promo;
		data["coins"]    = Coins;
		data["discount"] = Discount;
		data["no_ads"]   = NoAds;
		data["song_ids"] = SongIDs;
		data["order"]    = Order;
		
		return data;
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
			.OrderBy(_Snapshot => _Snapshot.Order)
			.ThenByDescending(_Snapshot => _Snapshot.Coins)
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Type == ProductType.Consumable)
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

	public async Task Upload()
	{
		Loaded = false;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (ProductSnapshot snapshot in m_Snapshots)
		{
			if (snapshot != null)
				data[snapshot.ID] = snapshot.Serialize();
		}
		
		await m_Data.SetValueAsync(data);
		
		await m_ProductsDescriptor.Upload();
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<ProductsDataUpdateSignal>();
	}

	public async Task Upload(params string[] _ProductIDs)
	{
		if (_ProductIDs == null || _ProductIDs.Length == 0)
			return;
		
		Loaded = false;
		
		foreach (string productID in _ProductIDs)
		{
			ProductSnapshot snapshot = GetSnapshot(productID);
			
			Dictionary<string, object> data = snapshot?.Serialize();
			
			await m_Data.Child(productID).SetValueAsync(data);
		}
		
		await m_ProductsDescriptor.Upload(_ProductIDs);
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<ProductsDataUpdateSignal>();
	}

	public void MoveSnapshot(string _NewsID, int _Offset)
	{
		int sourceIndex = m_Snapshots.FindIndex(_Snapshot => _Snapshot.ID == _NewsID);
		int targetIndex = sourceIndex + _Offset;
		
		if (sourceIndex < 0 || sourceIndex >= m_Snapshots.Count || targetIndex < 0 || targetIndex >= m_Snapshots.Count)
			return;
		
		(m_Snapshots[sourceIndex], m_Snapshots[targetIndex]) = (m_Snapshots[targetIndex], m_Snapshots[sourceIndex]);
		
		for (int i = 0; i < m_Snapshots.Count; i++)
			m_Snapshots[i].Order = i;
		
		m_SignalBus.Fire<ProductsDataUpdateSignal>();
	}

	public ProductSnapshot CreateSnapshot()
	{
		DatabaseReference reference = m_Data.Push();
		
		string productID = reference.Key;
		
		ProductSnapshot snapshot = new ProductSnapshot(productID);
		
		m_Snapshots.Insert(0, snapshot);
		
		m_ProductsDescriptor.CreateDescriptor(snapshot.ID);
		
		return snapshot;
	}

	public void RemoveSnapshot(string _ProductID)
	{
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == _ProductID);
		
		m_ProductsDescriptor.RemoveDescriptor(_ProductID);
		
		m_SignalBus.Fire<ProductsDataUpdateSignal>();
	}

	public ProductSnapshot GetSnapshot(string _ProductID)
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