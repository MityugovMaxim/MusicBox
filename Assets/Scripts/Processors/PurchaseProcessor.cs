using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;

public class PurchaseProcessor : IInitializable, IStoreListener
{
	const string PURCHASES_KEY = "PURCHASES";

	HashSet<string> m_Purchases = new HashSet<string>();

	bool Initialized => m_Controller != null && m_Extensions != null; 

	IStoreController   m_Controller;
	IExtensionProvider m_Extensions;

	readonly Dictionary<string, Action<string>> m_Success = new Dictionary<string, Action<string>>();
	readonly Dictionary<string, Action<string>> m_Failed  = new Dictionary<string, Action<string>>();

	public bool IsPurchased(string _ProductID)
	{
		if (!Initialized)
		{
			Debug.LogWarningFormat("[PurchaseProcessor] Check product failed. Store not initialized. Reading product with ID '{0}' from cache...", _ProductID);
			return m_Purchases.Contains(_ProductID);
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogWarningFormat("[PurchaseProcessor] Check product failed. Product with ID '{0}' not found. Reading product with ID '{0}' from cache...", _ProductID);
			return m_Purchases.Contains(_ProductID);
		}
		
		return !product.availableToPurchase && product.hasReceipt;
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
		
		return product.metadata.localizedPriceString;
	}

	public void Purchase(string _ProductID, Action<string> _Success = null, Action<string> _Failed = null)
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
		
		m_Success[_ProductID] = _Success;
		m_Failed[_ProductID]  = _Failed;
		
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

	void IInitializable.Initialize()
	{
		LoadPurchases();
		
		ConfigurationBuilder config = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		// TODO: Load products from scriptable object
		
		config.AddProduct("com.audiobox.pixies_where_is_my_mind", ProductType.NonConsumable);
		
		UnityPurchasing.Initialize(this, config);
	}

	void IStoreListener.OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
	{
		Debug.Log("[PurchaseProcessor] Initialize complete.");
		
		m_Controller = _Controller;
		m_Extensions = _Extensions;
		
		UpdatePurchases();
	}

	void IStoreListener.OnInitializeFailed(InitializationFailureReason _Reason)
	{
		Debug.LogErrorFormat("[PurchaseProcessor] Initialize failed. Reason: {0}.", _Reason);
	}

	PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs _Event)
	{
		string productID = _Event.purchasedProduct.definition.id;
		
		Debug.LogFormat("[PurchaseProcessor] Purchase complete. Product ID: '{0}'.", productID);
		
		m_Purchases.Add(productID);
		
		SavePurchases();
		
		InvokeSuccess(productID);
		
		return PurchaseProcessingResult.Complete;
	}

	void IStoreListener.OnPurchaseFailed(Product _Product, PurchaseFailureReason _Reason)
	{
		string productID = _Product.definition.id;
		
		Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product ID: '{0}'. Reason: {1}.", productID, _Reason);
		
		InvokeFailed(productID);
	}

	void LoadPurchases()
	{
		string data = PlayerPrefs.GetString(PURCHASES_KEY, string.Empty);
		
		// TODO: Decryption here
		
		string[] purchases = data.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
		
		m_Purchases = new HashSet<string>(purchases);
	}

	void UpdatePurchases()
	{
		if (!Initialized)
		{
			Debug.LogError("[PurchaseProcessor] Update purchases failed. Store not initialized.");
			return;
		}
		
		int count = m_Purchases.Count;
		
		foreach (Product product in m_Controller.products.all)
		{
			if (product == null || product.availableToPurchase || !product.hasReceipt)
				continue;
			
			string productID = product.definition.id;
			
			if (string.IsNullOrEmpty(productID))
				continue;
			
			if (!m_Purchases.Contains(productID))
				m_Purchases.Add(productID);
		}
		
		if (m_Purchases.Count != count)
			SavePurchases();
	}

	void SavePurchases()
	{
		string data = string.Join(";", m_Purchases.ToArray());
		
		// TODO: Encryption here
		
		PlayerPrefs.SetString(PURCHASES_KEY, data);
	}

	void InvokeSuccess(string _ProductID)
	{
		if (!m_Success.ContainsKey(_ProductID))
			return;
		
		if (m_Failed.ContainsKey(_ProductID))
			m_Failed.Remove(_ProductID);
		
		Action<string> action = m_Success[_ProductID];
		m_Success.Remove(_ProductID);
		action?.Invoke(_ProductID);
	}

	void InvokeFailed(string _ProductID)
	{
		if (!m_Failed.ContainsKey(_ProductID))
			return;
		
		if (m_Success.ContainsKey(_ProductID))
			m_Success.Remove(_ProductID);
		
		Action<string> action = m_Failed[_ProductID];
		m_Failed.Remove(_ProductID);
		action?.Invoke(_ProductID);
	}
}