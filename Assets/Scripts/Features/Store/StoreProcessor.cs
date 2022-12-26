using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;

[Preserve]
public class StoreProcessor : IStoreListener
{
	IStoreController m_Controller;

	Action<bool>         m_LoadFinished;
	Action<Product>      m_PurchaseSuccess;
	Action               m_PurchaseFailed;
	Action               m_PurchaseCanceled;
	Action<RequestState> m_ValidateFinished;

	readonly Dictionary<string, IDs> m_Products = new Dictionary<string, IDs>();

	public void Add(string _StoreID, IDs _IDs)
	{
		if (!string.IsNullOrEmpty(_StoreID) && _IDs != null)
			m_Products[_StoreID] = _IDs;
	}

	public void Remove(string _StoreID)
	{
		if (!string.IsNullOrEmpty(_StoreID) && m_Products.ContainsKey(_StoreID))
			m_Products.Remove(_StoreID);
	}

	public void Clear()
	{
		m_Products.Clear();
	}

	public Task Load()
	{
		m_Controller = null;
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		ConfigurationBuilder config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		foreach (var entry in m_Products)
		{
			config.AddProduct(
				entry.Key,
				UnityEngine.Purchasing.ProductType.Consumable,
				entry.Value
			);
		}
		
		m_LoadFinished = _Success => source.TrySetResult(_Success);
		
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

	public async Task<RequestState> Purchase(string _StoreID, string _ProductID, string _VoucherID)
	{
		try
		{
			Product product = await Purchase(_StoreID);
			
			return await Validate(product, _ProductID, _VoucherID);
		}
		catch (TaskCanceledException)
		{
			return RequestState.Cancel;
		}
		catch (Exception)
		{
			return RequestState.Fail;
		}
	}

	Task<Product> Purchase(string _StoreID)
	{
		Product product = GetProduct(_StoreID);
		
		TaskCompletionSource<Product> source = new TaskCompletionSource<Product>();
		
		Log.Info(this, "Purchasing '{0}'...", _StoreID);
		
		m_PurchaseSuccess  = _Product => source.TrySetResult(_Product);
		m_PurchaseCanceled = () => source.TrySetCanceled();
		m_PurchaseFailed   = () => source.TrySetException(new UnityException());
		
		m_Controller.InitiatePurchase(product);
		
		return source.Task;
	}

	async Task<RequestState> Validate(Product _Product, string _ProductID, string _VoucherID)
	{
		#if UNITY_IOS
		const string store = "AppStore";
		#elif UNITY_ANDROID
		const string store = "GooglePlay";
		#endif
		
		ProductPurchaseRequest request = new ProductPurchaseRequest(
			_ProductID,
			_VoucherID,
			_Product.receipt,
			store
		);
		
		bool success = await request.SendAsync();
		
		if (!success)
			return RequestState.Fail;
		
		m_Controller.ConfirmPendingPurchase(_Product);
		
		return RequestState.Success;
	}

	void InvokeLoadStoreFinished(bool _Success)
	{
		Action<bool> action = m_LoadFinished;
		m_LoadFinished = null;
		action?.Invoke(_Success);
	}

	void InvokePurchaseSuccess(Product _Product)
	{
		Action<Product> action = m_PurchaseSuccess;
		m_PurchaseSuccess = null;
		action?.Invoke(_Product);
	}

	void InvokePurchaseCanceled()
	{
		Action action = m_PurchaseCanceled;
		m_PurchaseSuccess = null;
		action?.Invoke();
	}

	void InvokePurchaseFailed()
	{
		Action action = m_PurchaseFailed;
		m_PurchaseFailed = null;
		action?.Invoke();
	}

	void IStoreListener.OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
	{
		Log.Info(this, "Initialize store complete.");
		
		m_Controller = _Controller;
		
		InvokeLoadStoreFinished(true);
	}

	void IStoreListener.OnInitializeFailed(InitializationFailureReason _Reason)
	{
		Log.Error(this, "Initialize store failed. Reason: {0}.", _Reason);
		
		m_Controller = null;
		
		InvokeLoadStoreFinished(false);
	}

	PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs _Event)
	{
		Product product = _Event.purchasedProduct;
		
		InvokePurchaseSuccess(product);
		
		return PurchaseProcessingResult.Pending;
	}

	void IStoreListener.OnPurchaseFailed(Product _Product, PurchaseFailureReason _Reason)
	{
		string productID = _Product.definition.id;
		
		Log.Error(this, "Purchase failed. Product ID: '{0}'. Reason: {1}.", productID, _Reason);
		
		if (_Reason == PurchaseFailureReason.ExistingPurchasePending)
			InvokePurchaseSuccess(_Product);
		else if (_Reason == PurchaseFailureReason.UserCancelled)
			InvokePurchaseCanceled();
		else
			InvokePurchaseFailed();
	}
}
