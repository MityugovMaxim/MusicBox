using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class StoreDataUpdateSignal { }

public class StoreProcessor : IStoreListener, IInitializable, IDisposable
{
	readonly SignalBus        m_SignalBus;
	readonly ProductProcessor m_ProductProcessor;

	IStoreController   m_Controller;
	IExtensionProvider m_Extensions;
	Action<bool>       m_LoadStoreFinished;
	Action<bool>       m_PurchaseFinished;

	[Inject]
	public StoreProcessor(
		SignalBus        _SignalBus,
		ProductProcessor _ProductProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_ProductProcessor = _ProductProcessor;
	}

	public Task LoadStore()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		List<string> productIDs = m_ProductProcessor.GetProductIDs();
		
		if (productIDs.Count == 0)
		{
			completionSource.TrySetResult(false);
			return completionSource.Task;
		}
		
		ConfigurationBuilder config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		foreach (string productID in productIDs)
		{
			Debug.LogFormat("[StoreProcessor] Initialize product '{0}'", productID);
			config.AddProduct(productID, m_ProductProcessor.GetType(productID));
		}
		
		m_LoadStoreFinished = _Success => completionSource.TrySetResult(_Success);
		
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
		
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[StoreProcessor] Purchase failed. Product ID is null or empty.");
			completionSource.TrySetResult(false);
			return completionSource.Task;
		}
		
		if (m_Controller == null)
		{
			Debug.LogError("[StoreProcessor] Purchase failed. Store is not loaded.");
			completionSource.TrySetResult(false);
			return completionSource.Task;
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
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
		
		m_Controller.InitiatePurchase(product);
		
		return completionSource.Task;
	}

	public string GetTitle(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[StoreProcessor] Get title failed. Product ID is null or empty.");
			return "-";
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get title failed. Product with ID '{0}' not found.", _ProductID);
			return "-";
		}
		
		return product.metadata.localizedTitle;
	}

	public string GetDescription(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[StoreProcessor] Get description failed. Product ID is null or empty.");
			return "-";
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get description failed. Product with ID '{0}' not found.", _ProductID);
			return "-";
		}
		
		return product.metadata.localizedDescription;
	}

	public string GetPrice(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogErrorFormat("[StoreProcessor] Get price failed. Product ID is null or empty.");
			return "-";
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogErrorFormat("[StoreProcessor] Get price failed. Product with ID '{0}' not found.", _ProductID);
			return "-";
		}
		
		return FormatPrice(product.metadata.localizedPrice, product.metadata.isoCurrencyCode, out string localizedPriceString)
			? localizedPriceString
			: product.metadata.localizedPriceString;
	}

	static bool FormatPrice(decimal _Price, string _CurrencyCode, out string _PriceString)
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

	async void Validate(Product _Product)
	{
		HttpsCallableReference validateReceipt = FirebaseFunctions.DefaultInstance.GetHttpsCallable("ValidateReceipt");
		
		Dictionary<string, object> data = new Dictionary<string, object>()
		{
			{ "product_id", _Product.definition.id },
			{ "receipt", _Product.receipt },
		};
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await validateReceipt.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch
		{
			Debug.LogError("[StoreProcessor] Validation failed.");
			
			success = false;
		}
		
		#if UNITY_EDITOR
		success = true;
		#endif
		
		if (success)
		{
			m_Controller.ConfirmPendingPurchase(_Product);
			
			InvokePurchaseFinished(true);
		}
		else
		{
			InvokePurchaseFinished(false);
		}
	}

	async void RegisterProfileDataUpdate()
	{
		await LoadStore();
		
		m_SignalBus.Fire<StoreDataUpdateSignal>();
	}

	async void RegisterProductDataUpdate()
	{
		await LoadStore();
		
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

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(RegisterProfileDataUpdate);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(RegisterProductDataUpdate);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(RegisterProfileDataUpdate);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(RegisterProductDataUpdate);
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
		
		InvokePurchaseFinished(false);
	}
}
