using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ProductDataUpdateSignal { }

public class ProductSnapshot
{
	public string                ID       { get; }
	public bool                  Active   { get; }
	public ProductType           Type     { get; }
	public bool                  Promo    { get; }
	public bool                  NoAds    { get; }
	public long                  Coins    { get; }
	public float                 Discount { get; }
	public IReadOnlyList<string> LevelIDs { get; }

	public ProductSnapshot(DataSnapshot _Data)
	{
		ID       = _Data.Key;
		Active   = _Data.GetBool("active");
		Type     = _Data.GetEnum<ProductType>("type");
		Promo    = _Data.GetBool("promo");
		Coins    = _Data.GetLong("coins");
		Discount = _Data.GetFloat("discount");
		NoAds    = _Data.GetBool("no_ads");
		LevelIDs = _Data.GetChildKeys("levels");
	}
}

public class ProductProcessor
{
	bool Loaded { get; set; }

	readonly SignalBus m_SignalBus;

	readonly List<ProductSnapshot> m_ProductSnapshots = new List<ProductSnapshot>();

	DatabaseReference      m_ProductsData;
	HttpsCallableReference m_ReceiptValidation;

	[Inject]
	public ProductProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public async Task LoadProducts()
	{
		if (m_ProductsData == null)
		{
			m_ProductsData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("products");
			m_ProductsData.ValueChanged += OnProductsUpdate;
		}
		
		await FetchProducts();
		
		Loaded = true;
	}

	public List<string> GetProductIDs()
	{
		return m_ProductSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public ProductType GetType(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetProductSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get type failed. Product snapshot with ID '{0}' is null.", _ProductID);
			return ProductType.Consumable;
		}
		
		return productSnapshot.Type;
	}

	public long GetCoins(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetProductSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get coins failed. Product snapshot with ID '{0}' is null.", _ProductID);
			return 0;
		}
		
		return productSnapshot.Coins;
	}

	public string GetCoinsProductID(long _Coins)
	{
		ProductSnapshot productSnapshot = m_ProductSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.OrderBy(_Snapshot => _Snapshot.Coins)
			.Aggregate((_A, _B) => _A.Coins < _B.Coins && _A.Coins >= _Coins ? _A : _B);
		
		return productSnapshot?.ID;
	}

	public float GetDiscount(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetProductSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get discount failed. Product snapshot with ID '{0}' is null.", _ProductID);
			return 0;
		}
		
		return productSnapshot.Discount;
	}

	public string[] GetLevelIDs(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetProductSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Get level IDs failed. Product snapshot is null for product with ID '{0}'", _ProductID);
			return Array.Empty<string>();
		}
		
		return productSnapshot.LevelIDs != null
			? productSnapshot.LevelIDs.ToArray()
			: Array.Empty<string>();
	}

	public bool HasLevel(string _LevelID)
	{
		if (m_ProductSnapshots.Count == 0)
			return false;
		
		return m_ProductSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.LevelIDs != null)
			.SelectMany(_Snapshot => _Snapshot.LevelIDs)
			.Contains(_LevelID);
	}

	public bool IsPromo(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetProductSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Promo check failed. Product snapshot with ID '{0}' is null.", _ProductID);
			return false;
		}
		
		return productSnapshot.Promo;
	}

	public bool IsNoAds(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetProductSnapshot(_ProductID);
		
		if (productSnapshot == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] No ads check failed. Product snapshot with ID '{0}' is null.", _ProductID);
			return false;
		}
		
		return productSnapshot.NoAds;
	}

	async void OnProductsUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[Score processor] Updating products data...");
		
		await FetchProducts();
		
		Debug.Log("[Score processor] Update products data complete.");
		
		m_SignalBus.Fire<ProductDataUpdateSignal>();
	}

	async Task FetchProducts()
	{
		m_ProductSnapshots.Clear();
		
		DataSnapshot productSnapshots = await m_ProductsData.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (productSnapshots == null)
		{
			Debug.LogError("[StoreProcessor] Fetch products failed.");
			return;
		}
		
		foreach (DataSnapshot productSnapshot in productSnapshots.Children)
		{
			ProductSnapshot product = new ProductSnapshot(productSnapshot);
			m_ProductSnapshots.Add(product);
		}
	}

	ProductSnapshot GetProductSnapshot(string _ProductID)
	{
		if (m_ProductSnapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[PurchaseProcessor] Get product snapshot failed. Product ID is null or empty.");
			return null;
		}
		
		return m_ProductSnapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _ProductID);
	}
}