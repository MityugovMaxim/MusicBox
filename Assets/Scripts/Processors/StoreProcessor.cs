using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class StoreProcessor : IStoreListener
{
	bool Loaded { get; set; }

	IStoreController     m_Controller;
	IExtensionProvider   m_Extensions;
	Action<bool>         m_LoadFinished;
	Action<RequestState> m_PurchaseFinished;

	Task m_Loading;

	readonly Dictionary<string, string> m_VoucherIDs = new SerializedDictionary<string, string>();

	Action m_StoreHandler;

	public void Subscribe(Action _Action) => m_StoreHandler += _Action;

	public void Unsubscribe(Action _Action) => m_StoreHandler -= _Action;

	public Task Load(StoreProduct[] _Products)
	{
		if (Loaded)
			return Task.CompletedTask;
		
		if (m_Loading != null)
			return m_Loading;
		
		if (_Products == null || _Products.Length == 0)
			return Task.FromResult(false);
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		m_Loading = source.Task;
		
		ConfigurationBuilder config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		foreach (StoreProduct product in _Products)
		{
			if (product == null)
				continue;
			
			Log.Info(this, "Initialize product with ID '{0}'.", product.ProductID);
			
			config.AddProduct(
				product.ProductID,
				UnityEngine.Purchasing.ProductType.Consumable,
				product.StoreIDs
			);
		}
		
		m_LoadFinished = _Success =>
		{
			Loaded = true;
			
			m_Loading = null;
			
			source.TrySetResult(_Success);
		};
		
		UnityPurchasing.Initialize(this, config);
		
		return source.Task;
	}

	public void Unload()
	{
		Loaded = false;
		
		m_Controller = null;
		m_Extensions = null;
	}

	public Task<bool> Restore()
	{
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		if (m_Extensions == null)
		{
			Debug.LogError("[StoreProcessor] Restore purchases failed. Store not initialized.");
			source.TrySetResult(false);
			return source.Task;
		}
		
		if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.OSXPlayer)
		{
			Debug.LogWarning("[StoreProcessor] Restore purchases is not supported.");
			source.TrySetResult(true);
			return source.Task;
		}
		
		IAppleExtensions apple = m_Extensions.GetExtension<IAppleExtensions>();
		
		if (apple == null)
		{
			Debug.LogError("[StoreProcessor] Restore purchases failed. Apple AppStore not found.");
			source.TrySetResult(false);
			return source.Task;
		}
		
		Debug.Log("[StoreProcessor] Restoring...");
		
		apple.RestoreTransactions(
			_Result =>
			{
				if (_Result)
				{
					Debug.Log("[StoreProcessor] Restore purchases complete.");
					
					source.TrySetResult(true);
				}
				else
				{
					Debug.LogError("[StoreProcessor] Restore purchases failed.");
					
					source.TrySetResult(false);
				}
			}
		);
		
		return source.Task;
	}

	public Task<RequestState> Purchase(string _ProductID, string _VoucherID)
	{
		if (string.IsNullOrEmpty(_ProductID))
			return Task.FromResult(RequestState.Fail);
		
		Product product = GetProduct(_ProductID);
		
		if (product == null)
		{
			Log.Error(this, "Purchase failed. Product with ID '{0}' not found.", _ProductID);
			return Task.FromResult(RequestState.Fail);
		}
		
		if (!product.availableToPurchase)
		{
			Log.Error(this, "Purchase failed. Product with ID '{0}' not available.", _ProductID);
			return Task.FromResult(RequestState.Fail);
		}
		
		TaskCompletionSource<RequestState> source = new TaskCompletionSource<RequestState>();
		
		Log.Info(this, "Purchasing product with ID '{0}'...", _ProductID);
		
		m_VoucherIDs[_ProductID] = _VoucherID;
		
		m_PurchaseFinished = _State => source.TrySetResult(_State);
		
		m_Controller.InitiatePurchase(product);
		
		return source.Task;
	}

	public string GetPrice(string _ProductID, bool _Sign = true)
	{
		Product product = GetProduct(_ProductID);
		
		if (product == null)
		{
			Log.Error(this, "Get price failed. Product with ID '{0}' not found.", _ProductID);
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
			Log.Error(this, "Get product failed. Product ID is null or empty.");
			return null;
		}
		
		if (m_Controller == null)
		{
			Log.Error(this, "Get product failed. Store is not loaded.");
			return null;
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
			Log.Error(this, "Get product failed. Product with ID '{0}' is null.", _ProductID);
		
		return product;
	}

	async void Validate(Product _Product)
	{
		#if UNITY_IOS
		const string store = "AppStore";
		#elif UNITY_ANDROID
		const string store = "GooglePlay";
		#endif
		
		string productID = _Product.definition.id;
		
		string voucherID = m_VoucherIDs.ContainsKey(productID)
			? m_VoucherIDs[productID]
			: null;
		
		ProductPurchaseRequest request = new ProductPurchaseRequest(
			productID,
			voucherID,
			_Product.receipt,
			store
		);
		
		bool success = await request.SendAsync();
		
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

	void InvokeLoadStoreFinished(bool _Success)
	{
		Action<bool> action = m_LoadFinished;
		m_LoadFinished = null;
		action?.Invoke(_Success);
		
		m_StoreHandler?.Invoke();
	}

	void InvokePurchaseFinished(bool _Success)
	{
		Action<RequestState> action = m_PurchaseFinished;
		m_PurchaseFinished = null;
		action?.Invoke(_Success ? RequestState.Success : RequestState.Fail);
	}

	void InvokePurchaseCanceled()
	{
		Action<RequestState> action = m_PurchaseFinished;
		m_PurchaseFinished = null;
		action?.Invoke(RequestState.Cancel);
	}

	void IStoreListener.OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
	{
		Log.Info(this, "Initialize store complete.");
		
		m_Controller = _Controller;
		m_Extensions = _Extensions;
		
		InvokeLoadStoreFinished(true);
	}

	void IStoreListener.OnInitializeFailed(InitializationFailureReason _Reason)
	{
		Log.Error(this, "Initialize store failed. Reason: {0}.", _Reason);
		
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
		
		Log.Error(this, "Purchase failed. Product ID: '{0}'. Reason: {1}.", productID, _Reason);
		
		if (_Reason== PurchaseFailureReason.UserCancelled)
			InvokePurchaseCanceled();
		else
			InvokePurchaseFinished(false);
	}
}
