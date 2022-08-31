using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class StoreDataUpdateSignal { }

[Preserve]
public class StoreProcessor : IStoreListener, IInitializable, IDisposable
{
	bool Loaded { get; set; }

	[Inject] SignalBus          m_SignalBus;
	[Inject] ProductsProcessor  m_ProductsProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	IStoreController   m_Controller;
	IExtensionProvider m_Extensions;
	Action<bool>       m_LoadStoreFinished;
	Action<bool>       m_PurchaseFinished;
	Action             m_PurchaseCanceled;

	TaskCompletionSource<bool> m_CompletionSource;

	public Task Load()
	{
		if (Loaded || m_CompletionSource != null)
			return m_CompletionSource?.Task ?? Task.CompletedTask;
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		List<string> productIDs = m_ProductsProcessor.GetProductIDs();
		
		if (productIDs.Count == 0)
		{
			completionSource.TrySetResult(false);
			
			return completionSource.Task;
		}
		
		ConfigurationBuilder config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		foreach (string productID in productIDs)
		{
			Log.Info(this, "Initialize product with ID '{0}'.", productID);
			
			config.AddProduct(
				productID,
				m_ProductsProcessor.GetType(productID) != ProductType.Subscription
					? ProductType.Consumable
					: ProductType.Subscription,
				m_ProductsProcessor.GetStoreIDs(productID)
			);
		}
		
		m_CompletionSource = completionSource;
		
		m_LoadStoreFinished = _Success =>
		{
			Loaded = true;
			
			m_CompletionSource.TrySetResult(_Success);
			
			m_CompletionSource = null;
		};
		
		UnityPurchasing.Initialize(this, config);
		
		return completionSource.Task;
	}

	public Task<bool> Restore()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (m_Extensions == null)
		{
			Debug.LogError("[StoreProcessor] Restore purchases failed. Store not initialized.");
			completionSource.TrySetResult(false);
			return completionSource.Task;
		}
		
		if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.OSXPlayer)
		{
			Debug.LogWarning("[StoreProcessor] Restore purchases is not supported.");
			completionSource.TrySetResult(true);
			return completionSource.Task;
		}
		
		IAppleExtensions apple = m_Extensions.GetExtension<IAppleExtensions>();
		
		if (apple == null)
		{
			Debug.LogError("[StoreProcessor] Restore purchases failed. Apple AppStore not found.");
			completionSource.TrySetResult(false);
			return completionSource.Task;
		}
		
		Debug.Log("[StoreProcessor] Restoring...");
		
		apple.RestoreTransactions(
			_Result =>
			{
				if (_Result)
				{
					Debug.Log("[StoreProcessor] Restore purchases complete.");
					
					completionSource.TrySetResult(true);
				}
				else
				{
					Debug.LogError("[StoreProcessor] Restore purchases failed.");
					
					completionSource.TrySetResult(false);
				}
			}
		);
		
		return completionSource.Task;
	}

	public Task<bool> Purchase(string _ProductID)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		Product product = GetProduct(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Purchase failed. Product with ID '{0}' not found.", _ProductID);
			completionSource.TrySetResult(false);
			return completionSource.Task;
		}
		
		if (!product.availableToPurchase)
		{
			Debug.LogErrorFormat("[StoreProcessor] Purchase failed. Product with ID '{0}' not available to purchase.", _ProductID);
			completionSource.TrySetResult(false);
			return completionSource.Task;
		}
		
		Debug.Log("[StoreProcessor] Purchasing...");
		
		m_PurchaseFinished = _Success => completionSource.TrySetResult(_Success);
		m_PurchaseCanceled = () => completionSource.TrySetCanceled();
		
		m_Controller.InitiatePurchase(product);
		
		return completionSource.Task;
	}

	public bool Subscribed(string _ProductID)
	{
		if (m_Extensions == null)
		{
			Log.Error(this, "Check subscription failed. Store is not initialized.");
			return false;
		}
		
		if (string.IsNullOrEmpty(_ProductID))
		{
			Log.Error(this, "Check subscription failed. Product ID null or empty.");
			return false;
		}
		
		Product product = GetProduct(_ProductID);
		
		if (product == null || product.definition.type != ProductType.Subscription)
		{
			Log.Error(this, "Check subscription failed. Product with ID '{0}' not found.", _ProductID);
			return false;
		}
		
		#if UNITY_IOS
		Dictionary<string, string> prices = m_Extensions
			.GetExtension<IAppleExtensions>()
			.GetIntroductoryPriceDictionary();
		
		if (!prices.TryGetValue(product.definition.storeSpecificId, out string data))
			return false;
		#elif UNITY_ANDROID
		string data = null;
		#endif
		
		SubscriptionManager manager = new SubscriptionManager(product, data);
		
		SubscriptionInfo subscription = manager.getSubscriptionInfo();
		
		if (subscription == null)
			return false;
		
		return subscription.isSubscribed() == Result.True;
	}

	public string GetPrice(string _ProductID, bool _Sign = true)
	{
		Product product = GetProduct(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get price failed. Product with ID '{0}' not found.", _ProductID);
			return "-";
		}
		
		return FormatPrice(product.metadata.localizedPrice, product.metadata.isoCurrencyCode, _Sign);
	}

	static string FormatPrice(decimal _Price, string _CurrencyCode, bool _Sign)
	{
		if (_Price.Equals(0m))
			return "-";
		
		string[] trim =
		{
			"RUB",
			"THB",
			"JPY",
			"IDR",
			"INR",
		};
		
		NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
		
		string code = _CurrencyCode ?? string.Empty;
		
		string sign;
		if (_Sign)
		{
			RegionInfo regionInfo = CultureInfo.GetCultures(CultureTypes.AllCultures)
				.Where(_CultureInfo => _CultureInfo.Name.Length > 0 && !_CultureInfo.IsNeutralCulture)
				.Select(_CultureInfo => new RegionInfo(_CultureInfo.LCID))
				.FirstOrDefault(_RegionInfo => string.Equals(_RegionInfo.ISOCurrencySymbol, code, StringComparison.InvariantCultureIgnoreCase));
			sign = regionInfo?.CurrencySymbol ?? code;
		}
		else
		{
			sign = code;
		}
		
		if (sign.Length >= 3)
		{
			numberFormatInfo.CurrencyPositivePattern = 2;
			numberFormatInfo.CurrencyNegativePattern = 12;
		}
		
		numberFormatInfo.CurrencySymbol = sign;
		
		if (trim.Contains(_CurrencyCode) && _Price - decimal.Truncate(_Price) < 0.001M)
			numberFormatInfo.CurrencyDecimalDigits = 0;
		
		return string.Format(numberFormatInfo, "{0:C}", _Price).Trim();
	}

	Product GetProduct(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[StoreProcessor] Get product failed. Product ID is null or empty.");
			return null;
		}
		
		if (m_Controller == null)
		{
			Debug.LogError("[StoreProcessor] Get product failed. Store is not loaded.");
			return null;
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
			Debug.LogErrorFormat("[StoreProcessor] Get product failed. Product with ID '{0}' is null.", _ProductID);
		
		return product;
	}

	async void Validate(Product _Product)
	{
		#if UNITY_IOS
		const string store = "AppStore";
		#elif UNITY_ANDROID
		const string store = "GooglePlay";
		#endif
		
		ProductPurchaseRequest request = new ProductPurchaseRequest(
			_Product.definition.id,
			_Product.receipt,
			store
		);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			m_Controller.ConfirmPendingPurchase(_Product);
			
			m_StatisticProcessor.LogPurchase(
				_Product.definition.storeSpecificId,
				_Product.metadata.isoCurrencyCode,
				_Product.metadata.localizedPrice
			);
			
			InvokePurchaseFinished(true);
		}
		else
		{
			InvokePurchaseFinished(false);
		}
	}

	async void RegisterProductDataUpdate()
	{
		Loaded = false;
		
		m_CompletionSource?.TrySetResult(false);
		
		await Load();
		
		m_SignalBus.Fire<StoreDataUpdateSignal>();
	}

	void InvokeLoadStoreFinished(bool _Success)
	{
		Action<bool> action = m_LoadStoreFinished;
		m_LoadStoreFinished = null;
		action?.Invoke(_Success);
	}

	void InvokePurchaseFinished(bool _Success)
	{
		Action<bool> action = m_PurchaseFinished;
		m_PurchaseFinished = null;
		action?.Invoke(_Success);
	}

	void InvokePurchaseCanceled()
	{
		Action action = m_PurchaseCanceled;
		m_PurchaseCanceled = null;
		action?.Invoke();
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(RegisterProductDataUpdate);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(RegisterProductDataUpdate);
	}

	void IStoreListener.OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
	{
		Debug.Log("[StoreProcessor] Initialize store complete.");
		
		m_Controller = _Controller;
		m_Extensions = _Extensions;
		
		InvokeLoadStoreFinished(true);
	}

	void IStoreListener.OnInitializeFailed(InitializationFailureReason _Reason)
	{
		Debug.LogErrorFormat("[StoreProcessor] Initialize store failed. Reason: {0}.", _Reason);
		
		m_Controller = null;
		m_Extensions = null;
		
		InvokeLoadStoreFinished(false);
	}

	PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs _Event)
	{
		Validate(_Event.purchasedProduct);
		
		return PurchaseProcessingResult.Pending;
	}

	void IStoreListener.OnPurchaseFailed(Product _Product, PurchaseFailureReason _Reason)
	{
		string productID = _Product.definition.id;
		
		Debug.LogErrorFormat("[StoreProcessor] Purchase failed. Product ID: '{0}'. Reason: {1}.", productID, _Reason);
		
		if (_Reason== PurchaseFailureReason.UserCancelled)
			InvokePurchaseCanceled();
		else
			InvokePurchaseFinished(false);
	}
}
