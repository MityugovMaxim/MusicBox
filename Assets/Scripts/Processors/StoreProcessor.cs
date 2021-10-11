using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;
using Zenject;

public class ProductDataUpdateSignal { }

public class PurchaseDataUpdateSignal { }

public class PurchaseSnapshot
{
	public string ID        { get; }
	public string ProductID { get; }
	public string Receipt   { get; }

	public PurchaseSnapshot(
		string _ID,
		string _ProductID,
		string _Receipt
	)
	{
		ID        = _ID;
		ProductID = _ProductID;
		Receipt   = _Receipt;
	}
}

public class ProductSnapshot
{
	public bool                  Promo    { get; }
	public IReadOnlyList<string> LevelIDs { get; }

	public ProductSnapshot(bool _Promo, IReadOnlyList<string> _LevelIDs)
	{
		Promo    = _Promo;
		LevelIDs = _LevelIDs;
	}
}

public class StoreProcessor : IStoreListener
{
	public bool Loaded { get; private set; }

	IStoreController   m_Controller;
	IExtensionProvider m_Extensions;

	readonly SignalBus       m_SignalBus;
	readonly SocialProcessor m_SocialProcessor;

	readonly List<string>                        m_ProductIDs        = new List<string>();
	readonly List<PurchaseSnapshot>              m_PurchaseSnapshots = new List<PurchaseSnapshot>();
	readonly Dictionary<string, ProductSnapshot> m_ProductSnapshots  = new Dictionary<string, ProductSnapshot>();

	DatabaseReference      m_ProductsData;
	DatabaseReference      m_PurchasesData;
	HttpsCallableReference m_ReceiptValidation;

	[Inject]
	public StoreProcessor(
		SignalBus       _SignalBus,
		SocialProcessor _SocialProcessor
	)
	{
		m_SignalBus       = _SignalBus;
		m_SocialProcessor = _SocialProcessor;
	}

	bool   m_LoadingStore;
	Action m_LoadStoreSuccess;
	Action m_LoadStoreFailed;
	Action m_PurchaseSuccess;
	Action m_PurchaseFailed;
	Action m_PurchaseCanceled;

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

	public Task LoadStore()
	{
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		if (Loaded)
		{
			taskSource.TrySetResult(true);
			return taskSource.Task;
		}
		
		void LoadStoreSuccess()
		{
			m_LoadingStore = false;
			
			taskSource.TrySetResult(true);
		}
		
		void LoadStoreFailed()
		{
			m_LoadingStore = false;
			
			taskSource.TrySetCanceled();
		}
		
		m_LoadStoreSuccess += LoadStoreSuccess;
		m_LoadStoreFailed  += LoadStoreFailed;
		
		if (m_LoadingStore)
			return taskSource.Task;
		
		m_LoadingStore = true;
		
		ConfigurationBuilder config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		foreach (string productID in m_ProductIDs)
		{
			Debug.LogFormat("[PurchaseProcessor] Initialize product '{0}'", productID);
			config.AddProduct(productID, ProductType.NonConsumable);
		}
		
		UnityPurchasing.Initialize(this, config);
		
		return taskSource.Task;
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

	public bool ContainsLevel(string _LevelID)
	{
		foreach (string productID in m_ProductIDs)
		{
			ProductSnapshot productSnapshot = GetProductSnapshot(productID);
			
			if (productSnapshot != null && productSnapshot.LevelIDs != null && productSnapshot.LevelIDs.Contains(_LevelID))
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

	public string GetPromoProductID()
	{
		return m_ProductIDs.SkipWhile(IsProductPurchased).FirstOrDefault(IsPromo);
	}

	public List<string> GetProductIDs()
	{
		return m_ProductIDs.SkipWhile(IsProductPurchased).ToList();
	}

	public bool IsPromo(string _ProductID)
	{
		ProductSnapshot productSnapshot = GetProductSnapshot(_ProductID);
		
		return productSnapshot != null && productSnapshot.Promo;
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
		if (!Loaded)
		{
			Debug.LogError("[PurchaseProcessor] Get title failed. Store is not loaded.");
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
		if (!Loaded)
		{
			Debug.LogError("[PurchaseProcessor] Get description failed. Store is not loaded.");
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
		if (!Loaded)
		{
			Debug.LogError("[PurchaseProcessor] Get price failed. Store is not loaded.");
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

	public async Task<bool> Purchase(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[PurchaseProcessor] Purchase failed. Product ID is null or empty.");
			return false;
		}
		
		if (!Loaded)
		{
			Debug.LogError("[PurchaseProcessor] Purchase failed. Store is not loaded.");
			return false;
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product with ID '{0}' not found.", _ProductID);
			return false;
		}
		
		if (!product.availableToPurchase)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product with ID '{0}' not available to purchase.", _ProductID);
			return false;
		}
		
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		void PurchaseSuccess()
		{
			taskSource.TrySetResult(true);
		}
		
		void PurchaseFailed()
		{
			taskSource.TrySetCanceled();
		}
		
		void PurchaseCanceled()
		{
			taskSource.TrySetCanceled();
		}
		
		m_PurchaseSuccess  = PurchaseSuccess;
		m_PurchaseFailed   = PurchaseFailed;
		m_PurchaseCanceled = PurchaseCanceled;
		
		m_Controller.InitiatePurchase(product);
		
		await taskSource.Task;
		
		return taskSource.Task.Result;
	}

	public void RestorePurchases(Action _Success = null, Action _Failed = null)
	{
		if (!Loaded)
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
		Loaded = false;
		
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
				productSnapshot.GetBool("promo"),
				productSnapshot.GetChildKeys("levels")
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
				purchaseSnapshot.GetString("receipt"),
				purchaseSnapshot.GetString("product_id")
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
		
		Loaded       = true;
		m_Controller = _Controller;
		m_Extensions = _Extensions;
		
		InvokeLoadStoreSuccess();
	}

	void IStoreListener.OnInitializeFailed(InitializationFailureReason _Reason)
	{
		Debug.LogErrorFormat("[PurchaseProcessor] Initialize failed. Reason: {0}.", _Reason);
		
		Loaded       = false;
		m_Controller = null;
		m_Extensions = null;
		
		InvokeLoadStoreFailed();
	}

	PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs _Event)
	{
		ReceiptValidation(_Event.purchasedProduct);
		
		return PurchaseProcessingResult.Pending;
	}

	void IStoreListener.OnPurchaseFailed(Product _Product, PurchaseFailureReason _Reason)
	{
		string productID = _Product.definition.id;
		
		Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product ID: '{0}'. Reason: {1}.", productID, _Reason);
		
		if (_Reason == PurchaseFailureReason.UserCancelled)
			InvokePurchaseCanceled();
		else
			InvokePurchaseFailed();
	}

	async void ReceiptValidation(Product _Product)
	{
		if (m_ReceiptValidation == null)
			m_ReceiptValidation = FirebaseFunctions.DefaultInstance.GetHttpsCallable("receipt_validation");
		
		Dictionary<string, object> data = new Dictionary<string, object>()
		{
			{ "product_id", _Product.definition.id },
			{ "receipt", _Product.receipt },
		};
		
		HttpsCallableResult result = await m_ReceiptValidation.CallAsync(data);
		
		data = result.Data as Dictionary<string, object>;
		
		bool success = data.GetBool("success");
		
		if (success)
		{
			m_Controller.ConfirmPendingPurchase(_Product);
			
			InvokePurchaseSuccess();
		}
		else
		{
			InvokePurchaseFailed();
		}
	}

	void InvokeLoadStoreSuccess()
	{
		Action action = m_LoadStoreSuccess;
		m_LoadStoreFailed  = null;
		m_LoadStoreSuccess = null;
		action?.Invoke();
	}

	void InvokeLoadStoreFailed()
	{
		Action action = m_LoadStoreFailed;
		m_LoadStoreFailed  = null;
		m_LoadStoreSuccess = null;
		action?.Invoke();
	}

	void InvokePurchaseSuccess()
	{
		Action action = m_PurchaseSuccess;
		m_PurchaseSuccess  = null;
		m_PurchaseFailed   = null;
		m_PurchaseCanceled = null;
		action?.Invoke();
	}

	void InvokePurchaseFailed()
	{
		Action action = m_PurchaseFailed;
		m_PurchaseSuccess  = null;
		m_PurchaseFailed   = null;
		m_PurchaseCanceled = null;
		action?.Invoke();
	}

	void InvokePurchaseCanceled()
	{
		Action action = m_PurchaseCanceled;
		m_PurchaseSuccess  = null;
		m_PurchaseFailed   = null;
		m_PurchaseCanceled = null;
		action?.Invoke();
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