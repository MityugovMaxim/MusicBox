using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

public class ProductDataUpdateSignal { }

public class PurchaseDataUpdateSignal { }

public class PurchaseProcessor : IStoreListener
{
	public bool Initialized => m_Controller != null && m_Extensions != null;

	public event Action<bool> OnInitialize
	{
		add
		{
			if (m_Resolved)
				value?.Invoke(Initialized);
			else
				m_OnInitialize += value;
		}
		remove => m_OnInitialize -= value;
	}

	bool               m_Resolved;
	IStoreController   m_Controller;
	IExtensionProvider m_Extensions;

	Action<bool> m_OnInitialize;

	readonly SignalBus       m_SignalBus;
	readonly SocialProcessor m_SocialProcessor;

	readonly List<string>                        m_ProductIDs        = new List<string>();
	readonly List<PurchaseSnapshot>              m_PurchaseSnapshots = new List<PurchaseSnapshot>();
	readonly Dictionary<string, ProductSnapshot> m_ProductSnapshots  = new Dictionary<string, ProductSnapshot>();

	readonly Dictionary<string, Action<string>> m_Success  = new Dictionary<string, Action<string>>();
	readonly Dictionary<string, Action<string>> m_Canceled = new Dictionary<string, Action<string>>();
	readonly Dictionary<string, Action<string>> m_Failed   = new Dictionary<string, Action<string>>();

	DatabaseReference m_ProductsData;
	DatabaseReference m_PurchasesData;

	[Inject]
	public PurchaseProcessor(
		SignalBus       _SignalBus,
		SocialProcessor _SocialProcessor
	)
	{
		m_SignalBus       = _SignalBus;
		m_SocialProcessor = _SocialProcessor;
	}

	public async Task LoadProducts()
	{
		if (m_ProductsData == null)
			m_ProductsData = FirebaseDatabase.DefaultInstance.RootReference.Child("products");
		
		await FetchProducts();
		
		m_ProductsData.ValueChanged += OnProductsUpdate;
	}

	public async Task LoadPurchases()
	{
		if (m_PurchasesData == null)
			m_PurchasesData = FirebaseDatabase.DefaultInstance.RootReference.Child("purchases").Child(m_SocialProcessor.UserID);
		
		await FetchPurchases();
		
		m_PurchasesData.ValueChanged += OnPurchasesUpdate;
	}

	public void LoadStore()
	{
		m_Resolved = false;
		
		ConfigurationBuilder config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		foreach (string productID in m_ProductIDs)
		{
			Debug.LogFormat("[PurchaseProcessor] Initialize product '{0}'", productID);
			config.AddProduct(productID, ProductType.NonConsumable);
		}
		
		UnityPurchasing.Initialize(this, config);
	}

	public bool IsProductPurchased(string _ProductID)
	{
		foreach (PurchaseSnapshot purchaseSnapshot in m_PurchaseSnapshots)
		{
			string productID = purchaseSnapshot.ProductID;
			
			if (productID == _ProductID)
				return true;
		}
		return false;
	}

	public bool IsLevelPurchased(string _LevelID)
	{
		HashSet<string> levelIDs = new HashSet<string>();
		foreach (string productID in m_ProductIDs)
		{
			ProductSnapshot productSnapshot = GetProductSnapshot(productID);
			
			if (productSnapshot == null || productSnapshot.LevelIDs == null)
				continue;
			
			foreach (string levelID in productSnapshot.LevelIDs)
				levelIDs.Add(levelID);
		}
		
		if (!levelIDs.Contains(_LevelID))
			return true;
		
		foreach (PurchaseSnapshot purchaseSnapshot in m_PurchaseSnapshots)
		{
			string productID = purchaseSnapshot.ProductID;
			
			if (string.IsNullOrEmpty(productID))
				continue;
			
			ProductSnapshot productSnapshot = GetProductSnapshot(productID);
			
			if (productSnapshot == null || productSnapshot.LevelIDs == null)
				continue;
			
			if (productSnapshot.LevelIDs.Contains(_LevelID))
				return true;
		}
		return false;
	}

	public string[] GetProductIDs()
	{
		return m_ProductIDs.SkipWhile(IsProductPurchased).ToArray();
	}

	public string GetNextProductID(string _ProductID)
	{
		int index = m_ProductIDs.IndexOf(_ProductID);
		
		if (index < 0)
			return _ProductID;
		
		for (int i = 1; i <= m_ProductIDs.Count; i++)
		{
			int j = MathUtility.Repeat(index + i, m_ProductIDs.Count);
			
			string productID = m_ProductIDs[j];
			
			if (IsProductPurchased(productID))
				continue;
			
			return productID;
		}
		
		return _ProductID;
	}

	public string GetPreviousProductID(string _ProductID)
	{
		int index = m_ProductIDs.IndexOf(_ProductID);
		
		if (index < 0)
			return _ProductID;
		
		for (int i = 1; i <= m_ProductIDs.Count; i++)
		{
			int j = MathUtility.Repeat(index - i, m_ProductIDs.Count);
			
			string productID = m_ProductIDs[j];
			
			if (IsProductPurchased(productID))
				continue;
			
			return productID;
		}
		
		return _ProductID;
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

	public string GetTitle(string _ProductID)
	{
		if (!Initialized)
		{
			Debug.LogError("[PurchaseProcessor] Get title failed. Store not initialized.");
			return "-";
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Get title failed. Product with ID '{0}' not found.", _ProductID);
			return "-";
		}
		
		return product.metadata.localizedTitle;
	}

	public string GetDescription(string _ProductID)
	{
		if (!Initialized)
		{
			Debug.LogError("[PurchaseProcessor] Get description failed. Store not initialized.");
			return "-";
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Get description failed. Product with ID '{0}' not found.", _ProductID);
			return "-";
		}
		
		return product.metadata.localizedDescription;
	}

	public string GetPrice(string _ProductID)
	{
		if (!Initialized)
		{
			Debug.LogError("[PurchaseProcessor] Get price failed. Store not initialized.");
			return "-";
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Get price failed. Product with ID '{0}' not found.", _ProductID);
			return "-";
		}
		
		return TryGetPrice(product.metadata.localizedPrice, product.metadata.isoCurrencyCode, out string localizedPriceString)
			? localizedPriceString
			: product.metadata.localizedPriceString;
	}

	public void Purchase(
		string         _ProductID,
		Action<string> _Success  = null,
		Action<string> _Canceled = null,
		Action<string> _Failed   = null
	)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[PurchaseProcessor] Purchase failed. Product ID is null or empty.");
			_Failed?.Invoke(_ProductID);
			return;
		}
		
		if (!Initialized)
		{
			Debug.LogError("[PurchaseProcessor] Purchase failed. Store not initialized.");
			_Failed?.Invoke(_ProductID);
			return;
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product with ID '{0}' not found.", _ProductID);
			_Failed?.Invoke(_ProductID);
			return;
		}
		
		if (!product.availableToPurchase)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product with ID '{0}' not available to purchase.", _ProductID);
			_Failed?.Invoke(_ProductID);
			return;
		}
		
		m_Success[_ProductID]  = _Success;
		m_Canceled[_ProductID] = _Canceled;
		m_Failed[_ProductID]   = _Failed;
		
		m_Controller.InitiatePurchase(product);
	}

	public void RestorePurchases(Action _Success = null, Action _Failed = null)
	{
		if (!Initialized)
		{
			Debug.LogError("[PurchaseProcessor] Restore purchases failed. Store not initialized.");
			_Failed?.Invoke();
			return;
		}
		
		if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.OSXPlayer)
		{
			_Success?.Invoke();
			return;
		}
		
		IAppleExtensions apple = m_Extensions.GetExtension<IAppleExtensions>();
		
		if (apple == null)
		{
			Debug.LogError("[PurchaseProcessor] Restore purchases failed. Apple AppStore not found.");
			_Failed?.Invoke();
			return;
		}
		
		apple.RestoreTransactions(
			_Result =>
			{
				if (_Result)
				{
					Debug.Log("[PurchaseProcessor] Restore purchases complete.");
					
					_Success?.Invoke();
				}
				else
				{
					Debug.LogError("[PurchaseProcessor] Restore purchases failed.");
					
					_Failed?.Invoke();
				}
			}
		);
	}

	async void OnProductsUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[Score processor] Updating products data...");
		
		await FetchProducts();
		
		Debug.Log("[Score processor] Update products data complete.");
		
		m_SignalBus.Fire<ProductDataUpdateSignal>();
	}

	async void OnPurchasesUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[Score processor] Updating purchases data...");
		
		await FetchPurchases();
		
		Debug.Log("[Score processor] Update purchases data complete.");
		
		m_SignalBus.Fire<ProductDataUpdateSignal>();
	}

	async Task FetchProducts()
	{
		m_ProductIDs.Clear();
		m_ProductSnapshots.Clear();
		
		DataSnapshot productsSnapshot = await m_ProductsData.GetValueAsync();
		
		foreach (DataSnapshot productSnapshot in productsSnapshot.Children)
		{
			string productID = productSnapshot.Key;
			ProductSnapshot product = new ProductSnapshot(
				productSnapshot.Child("levels").GetChildKeys()
			);
			m_ProductIDs.Add(productID);
			m_ProductSnapshots[productID] = product;
		}
	}

	async Task FetchPurchases()
	{
		m_PurchaseSnapshots.Clear();
		
		DataSnapshot purchasesSnapshot = await m_PurchasesData.GetValueAsync();
		
		foreach (DataSnapshot purchaseSnapshot in purchasesSnapshot.Children)
		{
			PurchaseSnapshot purchase = new PurchaseSnapshot(
				purchaseSnapshot.Key,
				purchaseSnapshot.Child("receipt").GetString(),
				purchaseSnapshot.Child("product_id").GetString()
			);
			m_PurchaseSnapshots.Add(purchase);
		}
	}

	static bool TryGetPrice(decimal _Price, string _CurrencyCode, out string _PriceString)
	{
		RegionInfo regionInfo = CultureInfo.GetCultures(CultureTypes.AllCultures)
			.Where(_CultureInfo => _CultureInfo.Name.Length > 0 && !_CultureInfo.IsNeutralCulture)
			.Select(_CultureInfo => new RegionInfo(_CultureInfo.LCID))
			.FirstOrDefault(_RegionInfo => string.Equals(_RegionInfo.ISOCurrencySymbol, _CurrencyCode, StringComparison.InvariantCultureIgnoreCase));
		
		if (regionInfo == null)
		{
			_PriceString = null;
			return false;
		}
		
		NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
		numberFormatInfo.CurrencySymbol = regionInfo.CurrencySymbol;
		
		if (_CurrencyCode == "RUB" || _CurrencyCode == "JPY" || _CurrencyCode == "IDR" || _CurrencyCode == "INR")
		{
			if (_Price - decimal.Truncate(_Price) < 0.001M)
				numberFormatInfo.CurrencyDecimalDigits = 0;
		}
		
		_PriceString = string.Format(numberFormatInfo, "{0:C}", _Price);
		
		return true;
	}

	void IStoreListener.OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
	{
		Debug.Log("[PurchaseProcessor] Initialize complete.");
		
		m_Controller = _Controller;
		m_Extensions = _Extensions;
		
		m_Resolved = true;
		
		Action<bool> action = m_OnInitialize;
		m_OnInitialize = null;
		action?.Invoke(true);
	}

	void IStoreListener.OnInitializeFailed(InitializationFailureReason _Reason)
	{
		Debug.LogErrorFormat("[PurchaseProcessor] Initialize failed. Reason: {0}.", _Reason);
		
		m_Resolved = true;
		
		Action<bool> action = m_OnInitialize;
		m_OnInitialize = null;
		action?.Invoke(false);
	}

	PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs _Event)
	{
		SavePurchase(_Event.purchasedProduct);
		
		return PurchaseProcessingResult.Pending;
	}

	void IStoreListener.OnPurchaseFailed(Product _Product, PurchaseFailureReason _Reason)
	{
		string productID = _Product.definition.id;
		
		Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product ID: '{0}'. Reason: {1}.", productID, _Reason);
		
		if (_Reason == PurchaseFailureReason.UserCancelled)
			InvokePurchaseCancelled(productID);
		else
			InvokePurchaseFailed(productID);
	}

	async void SavePurchase(Product _Product)
	{
		string productID = _Product.definition.id;
		
		Debug.LogFormat("[PurchaseProcessor] Purchase complete. Product ID: '{0}'.", productID);
		
		IDictionary<string, object> data = new Dictionary<string, object>()
		{
			{ "product_id", _Product.definition.id },
			{ "receipt", _Product.receipt },
		};
		
		await m_PurchasesData.Child(_Product.transactionID).SetValueAsync(data);
		
		m_Controller.ConfirmPendingPurchase(_Product);
		
		InvokePurchaseSuccess(productID);
	}

	void InvokePurchaseSuccess(string _ProductID)
	{
		if (!m_Success.ContainsKey(_ProductID))
			return;
		
		if (m_Canceled.ContainsKey(_ProductID))
			m_Canceled.Remove(_ProductID);
		
		if (m_Failed.ContainsKey(_ProductID))
			m_Failed.Remove(_ProductID);
		
		Action<string> action = m_Success[_ProductID];
		m_Success.Remove(_ProductID);
		action?.Invoke(_ProductID);
	}

	void InvokePurchaseCancelled(string _ProductID)
	{
		if (!m_Canceled.ContainsKey(_ProductID))
			return;
		
		if (m_Success.ContainsKey(_ProductID))
			m_Success.Remove(_ProductID);
		
		if (m_Failed.ContainsKey(_ProductID))
			m_Failed.Remove(_ProductID);
		
		Action<string> action = m_Canceled[_ProductID];
		m_Canceled.Remove(_ProductID);
		action?.Invoke(_ProductID);
	}

	void InvokePurchaseFailed(string _ProductID)
	{
		if (!m_Failed.ContainsKey(_ProductID))
			return;
		
		if (m_Success.ContainsKey(_ProductID))
			m_Success.Remove(_ProductID);
		
		if (m_Canceled.ContainsKey(_ProductID))
			m_Canceled.Remove(_ProductID);
		
		Action<string> action = m_Failed[_ProductID];
		m_Failed.Remove(_ProductID);
		action?.Invoke(_ProductID);
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