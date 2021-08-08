using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing;
using Zenject;
using Random = UnityEngine.Random;

public class PurchaseProcessor : IInitializable, IStoreListener
{
	const string PURCHASES_KEY = "PURCHASES";
	const string HASH_KEY      = "PURCHASES_HASH";
	const string SECRET_KEY    = "PURCHASES_SECRET";

	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	static readonly string[] m_Secret =
	{
		"9Swtr24jaf",
		"7shFhj*ef3",
		"Vy;*Sjflq)",
		"mvkPS94M#*",
		"l7AHvm0J*@",
	};

	public bool Initialized => m_Controller != null && m_Extensions != null;

	IStoreController   m_Controller;
	IExtensionProvider m_Extensions;

	readonly SignalBus                          m_SignalBus;
	readonly List<string>                       m_ProductIDs         = new List<string>();
	readonly Dictionary<string, ProductInfo>    m_ProductInfos       = new Dictionary<string, ProductInfo>();
	readonly Dictionary<string, Sprite>         m_PreviewThumbnails  = new Dictionary<string, Sprite>();
	readonly Dictionary<string, Sprite>         m_PreviewBackgrounds = new Dictionary<string, Sprite>();
	readonly Dictionary<string, Action<string>> m_Success            = new Dictionary<string, Action<string>>();
	readonly Dictionary<string, Action<string>> m_Failed             = new Dictionary<string, Action<string>>();
	readonly HashSet<string>                    m_Purchases          = new HashSet<string>();

	[Inject]
	public PurchaseProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
		
		ProductRegistry productRegistry = Registry.Load<ProductRegistry>("product_registry");
		if (productRegistry != null)
		{
			foreach (ProductInfo productInfo in productRegistry)
			{
				if (productInfo == null || !productInfo.Active)
					continue;
				
				m_ProductIDs.Add(productInfo.ID);
				m_ProductInfos[productInfo.ID] = productInfo;
			}
		}
		
		LoadPurchases();
	}

	void IInitializable.Initialize()
	{
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
		if (m_Purchases.Contains(_ProductID))
			return true;
		
		if (!Initialized)
		{
			Debug.LogWarningFormat("[PurchaseProcessor] Check product failed. Store not initialized. Product ID: '{0}'.", _ProductID);
			return false;
		}
		
		Product product = m_Controller.products.WithID(_ProductID);
		
		if (product == null)
		{
			Debug.LogWarningFormat("[PurchaseProcessor] Check product failed. Product with ID '{0}' not found.", _ProductID);
			return false;
		}
		
		return product.hasReceipt;
	}

	public bool IsLevelPurchased(string _LevelID)
	{
		List<ProductInfo> productInfos = new List<ProductInfo>();
		foreach (string productID in m_ProductIDs)
		{
			if (string.IsNullOrEmpty(productID) || !m_ProductInfos.ContainsKey(productID))
				continue;
			
			ProductInfo productInfo = m_ProductInfos[productID];
			
			if (productInfo != null && productInfo.ContainsLevel(_LevelID))
				productInfos.Add(productInfo);
		}
		return productInfos.Count == 0 || productInfos.Any(_ProductInfo => IsProductPurchased(_ProductInfo.ID));
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
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[PurchaseProcessor] Get level IDs failed. Product ID is null or empty.");
			return new string[0];
		}
		
		if (!m_ProductInfos.ContainsKey(_ProductID))
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Get level IDs failed. Product info not found for product with ID '{0}'.", _ProductID);
			return new string[0];
		}
		
		ProductInfo productInfo = m_ProductInfos[_ProductID];
		
		if (productInfo == null)
		{
			Debug.LogErrorFormat("[PurchaseProcessor] Get level IDs failed. Product info is null for product with ID '{0}'", _ProductID);
			return new string[0];
		}
		
		if (productInfo.LevelInfos == null || productInfo.LevelInfos.Length == 0)
			return new string[0];
		
		return productInfo.LevelInfos.Select(_LevelInfo => _LevelInfo.ID).ToArray();
	}

	public Sprite GetPreviewBackground(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[PurchaseProcessor] Get preview background failed. Product ID is null or empty.");
			return null;
		}
		
		if (m_PreviewBackgrounds.ContainsKey(_ProductID) && m_PreviewBackgrounds[_ProductID] != null)
			return m_PreviewBackgrounds[_ProductID];
		
		Sprite previewThumbnail = GetPreviewThumbnail(_ProductID);
		
		Sprite previewBackground = BlurUtility.Blur(previewThumbnail, 0.8f, 5);
		
		m_PreviewBackgrounds[_ProductID] = previewBackground;
		
		return previewBackground;
	}

	public Sprite GetPreviewThumbnail(string _ProductID)
	{
		if (string.IsNullOrEmpty(_ProductID))
		{
			Debug.LogError("[PurchaseProcessor] Get preview thumbnail failed. Product ID is null or empty.");
			return null;
		}
		
		if (m_PreviewThumbnails.ContainsKey(_ProductID) && m_PreviewThumbnails[_ProductID] != null)
			return m_PreviewThumbnails[_ProductID];
		
		string path = $"{_ProductID}/preview_thumbnail";
		
		Sprite previewThumbnail = Resources.Load<Sprite>(path);
		
		m_PreviewThumbnails[_ProductID] = previewThumbnail;
		
		return previewThumbnail;
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
		
		InvokePurchaseSuccess(productID);
		
		m_SignalBus.Fire(new PurchaseSignal(productID));
		
		return PurchaseProcessingResult.Complete;
	}

	void IStoreListener.OnPurchaseFailed(Product _Product, PurchaseFailureReason _Reason)
	{
		string productID = _Product.definition.id;
		
		Debug.LogErrorFormat("[PurchaseProcessor] Purchase failed. Product ID: '{0}'. Reason: {1}.", productID, _Reason);
		
		InvokePurchaseFailed(productID);
	}

	public static string GetHash(string _Value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(_Value);
		
		MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
		
		byte[] hashBytes = hashProvider.ComputeHash(bytes);
		
		StringBuilder hash = new StringBuilder();
		foreach (byte hashByte in hashBytes)
			hash.Append(Convert.ToString(hashByte, 16).PadLeft(2, '0'));
		return hash.ToString().PadLeft(32, '0');
	}

	void LoadPurchases()
	{
		m_Purchases.Clear();
		
		string data = PlayerPrefs.GetString(PURCHASES_KEY, string.Empty);
		
		if (string.IsNullOrEmpty(data))
			return;
		
		int secret = PlayerPrefs.GetInt(SECRET_KEY, -1);
		
		if (secret < 0 || secret >= m_Secret.Length)
		{
			Debug.LogError("[PurchaseProcessor] Load purchases failed. Invalid secret key.");
			return;
		}
		
		string hash = PlayerPrefs.GetString(HASH_KEY, string.Empty);
		
		if (string.IsNullOrEmpty(hash))
		{
			Debug.LogError("[PurchaseProcessor] Load purchases failed. Invalid hash.");
			return;
		}
		
		string key = m_Secret[secret];
		
		if (hash != GetHash($"{secret}_{key}_{data}"))
		{
			Debug.LogError("[PurchaseProcessor] Load purchases failed. Hash mismatch.");
			return;
		}
		
		Debug.LogFormat("[PurchaseProcessor] Purchases loaded. Hash: {0} Secret: {1}", hash, key);
		
		string[] purchases = data.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
		
		foreach (string purchase in purchases)
		{
			Debug.LogFormat("[PurchaseProcessor] Loaded purchase: " + purchase);
			m_Purchases.Add(purchase);
		}
	}

	void SavePurchases()
	{
		string data   = string.Join(";", m_Purchases.ToArray());
		int    secret = Random.Range(0, m_Secret.Length);
		string key    = m_Secret[secret];
		string hash   = GetHash($"{secret}_{key}_{data}");
		
		PlayerPrefs.SetString(PURCHASES_KEY, data);
		PlayerPrefs.SetInt(SECRET_KEY, secret);
		PlayerPrefs.SetString(HASH_KEY, hash);
	}

	void UpdatePurchases()
	{
		#if UNITY_EDITOR
		if (!Initialized)
		{
			Debug.LogError("[PurchaseProcessor] Update purchases failed. Store not initialized.");
			return;
		}
		
		int count = m_Purchases.Count;
		
		foreach (Product product in m_Controller.products.all)
		{
			if (product == null || !product.hasReceipt)
				continue;
			
			string productID = product.definition.id;
			
			if (string.IsNullOrEmpty(productID))
				continue;
			
			if (!m_Purchases.Contains(productID))
				m_Purchases.Add(productID);
		}
		
		if (m_Purchases.Count != count)
			SavePurchases();
		#else
		m_Purchases.Clear();
		foreach (Product product in m_Controller.products.all)
		{
			if (product != null && product.hasReceipt)
				m_Purchases.Add(product.definition.id);
		}
		SavePurchases();
		#endif
	}

	void InvokePurchaseSuccess(string _ProductID)
	{
		if (!m_Success.ContainsKey(_ProductID))
			return;
		
		if (m_Failed.ContainsKey(_ProductID))
			m_Failed.Remove(_ProductID);
		
		Action<string> action = m_Success[_ProductID];
		m_Success.Remove(_ProductID);
		action?.Invoke(_ProductID);
	}

	void InvokePurchaseFailed(string _ProductID)
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