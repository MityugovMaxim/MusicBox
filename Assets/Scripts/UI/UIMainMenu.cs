using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Functions;
using UnityEngine;
using Zenject;

public enum MainMenuPageType
{
	News    = 0,
	Store   = 1,
	Levels  = 2,
	Profile = 3,
	Offers  = 4,
}

[Menu(MenuType.MainMenu)]
public class UIMainMenu : UIMenu
{
	// TODO: Remove
	async void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
			resultMenu.Setup("mindme_my_home_is_you");
			await m_MenuProcessor.Show(MenuType.ResultMenu);
		}
		
		if (Input.GetKeyDown(KeyCode.F))
		{
			await UnlockLevelFunction();
			await CompleteLevelFunction();
			await ValidateReceiptFunction();
		}
	}

	async Task UnlockLevelFunction()
	{
		Debug.LogError("---> UNLOCK LEVEL FUNCTION");
		
		HttpsCallableReference unlockLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("UnlockLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"] = "mindme_my_home_is_you";
		
		HttpsCallableResult result = await unlockLevel.CallAsync(data);
		
		Debug.LogError("---> UnlockLevel function result: " + result.Data);
	}

	async Task CompleteLevelFunction()
	{
		Debug.LogError("---> FINISH LEVEL FUNCTION");
		
		HttpsCallableReference finishLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("FinishLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"] = "sandro_beach_bonfire";
		data["rank"]     = (int)ScoreRank.Platinum;
		data["score"]    = 100;
		data["accuracy"] = 100;
		
		HttpsCallableResult result = await finishLevel.CallAsync(data);
		
		Debug.LogError("---> FinishLevel function result: " + result.Data);
	}

	async Task ValidateReceiptFunction()
	{
		Debug.LogError("---> VALIDATE RECEIPT FUNCTION");
		
		HttpsCallableReference validateReceipt = FirebaseFunctions.DefaultInstance.GetHttpsCallable("ValidateReceipt");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["product_id"] = "no_ads";
		data["receipt"]    = "a0aoeigomwoimeomsldkm";
		
		HttpsCallableResult result = await validateReceipt.CallAsync(data);
		
		Debug.LogError("---> ValidateReceipt function result: " + result.Data);
	}

	[SerializeField] UIProductPromo    m_ProductPromo;
	[SerializeField] UIMainMenuPage[]  m_Pages;
	[SerializeField] UIMainMenuControl m_Control;

	SignalBus        m_SignalBus;
	ProfileProcessor m_ProfileProcessor;
	ProductProcessor m_ProductProcessor;
	UrlProcessor     m_UrlProcessor;
	MenuProcessor    m_MenuProcessor;

	[NonSerialized] MainMenuPageType m_PageType = MainMenuPageType.Levels;

	[Inject]
	public void Construct(
		SignalBus        _SignalBus,
		ProfileProcessor _ProfileProcessor,
		ProductProcessor _ProductProcessor,
		UrlProcessor     _UrlProcessor,
		MenuProcessor    _MenuProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_ProfileProcessor = _ProfileProcessor;
		m_ProductProcessor = _ProductProcessor;
		m_UrlProcessor     = _UrlProcessor;
		m_MenuProcessor    = _MenuProcessor;
	}

	public void Select(MainMenuPageType _PageType)
	{
		Select(_PageType, false);
	}

	public void Select(MainMenuPageType _PageType, bool _Instant)
	{
		if (m_PageType == _PageType)
			return;
		
		m_PageType = _PageType;
		
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == m_PageType)
				page.Show(_Instant);
			else
				page.Hide(_Instant);
		}
		
		m_Control.Select(m_PageType, _Instant);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == m_PageType)
				page.Show(true);
			else
				page.Hide(true);
		}
		m_Control.Select(m_PageType, true);
		
		m_SignalBus.Subscribe<PurchaseSignal>(Refresh);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(Refresh);
		
		Application.deepLinkActivated += ProcessDeepLink;
	}

	protected override void OnHideStarted()
	{
		Application.deepLinkActivated -= ProcessDeepLink;
	}

	protected override void OnHideFinished()
	{
		foreach (UIMainMenuPage page in m_Pages)
			page.Hide(true);
		
		m_SignalBus.Unsubscribe<PurchaseSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		string productID = m_ProfileProcessor.GetVisibleProductIDs().FirstOrDefault(m_ProductProcessor.IsPromo);
		
		if (string.IsNullOrEmpty(productID))
			return;
		
		m_ProductPromo.Setup(productID);
	}

	async void ProcessDeepLink(string _URL)
	{
		await m_UrlProcessor.ProcessURL(_URL);
	}
}
