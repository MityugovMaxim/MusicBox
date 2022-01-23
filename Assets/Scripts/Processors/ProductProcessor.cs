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
	public ProductType           Type     { get; }
	public bool                  Promo    { get; }
	public bool                  NoAds    { get; }
	public long                  Coins    { get; }
	public float                 Discount { get; }
	public IReadOnlyList<string> LevelIDs { get; }

	public ProductSnapshot(DataSnapshot _Data)
	{
		ID       = _Data.Key;
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

	readonly List<string>                        m_ProductIDs       = new List<string>();
	readonly Dictionary<string, ProductSnapshot> m_ProductSnapshots = new Dictionary<string, ProductSnapshot>();

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
		return m_ProductIDs.ToList();
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
		foreach (string productID in m_ProductIDs)
		{
			ProductSnapshot productSnapshot = GetProductSnapshot(productID);
			
			if (productSnapshot == null || productSnapshot.LevelIDs == null)
				continue;
			
			if (productSnapshot.LevelIDs.Contains(_LevelID))
				return true;
		}
		return false;
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
		m_ProductIDs.Clear();
		m_ProductSnapshots.Clear();
		
		DataSnapshot productSnapshots = await m_ProductsData.GetValueAsync(15000, 2);
		
		if (productSnapshots == null)
		{
			Debug.LogError("[StoreProcessor] Fetch products failed.");
			return;
		}
		
		foreach (DataSnapshot productSnapshot in productSnapshots.Children)
		{
			bool active = productSnapshot.GetBool("active");
			
			if (!active)
				continue;
			
			ProductSnapshot product = new ProductSnapshot(productSnapshot);
			
			m_ProductIDs.Add(product.ID);
			m_ProductSnapshots[product.ID] = product;
		}
	}

	ProductSnapshot GetProductSnapshot(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[PurchaseProcessor] Get product snapshot failed. Product ID is null or empty.");
			return null;
		}
		
		if (!m_ProductSnapshots.ContainsKey(_ProductID))
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Get product snapshot failed. Product snapshot not found for product with ID '{0}'.", _ProductID);
			return null;
		}
		
		return m_ProductSnapshots[_ProductID];
	}
}