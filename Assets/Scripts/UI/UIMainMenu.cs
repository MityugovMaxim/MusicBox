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
public class UIMainMenu : UIMenu, IInitializable
{
	[SerializeField] UIProductPromo    m_ProductPromo;
	[SerializeField] UIMainMenuPage[]  m_Pages;
	[SerializeField] UIMainMenuControl m_Control;

	SignalBus      m_SignalBus;
	StoreProcessor m_StoreProcessor;

	[Inject]
	public void Construct(
		SignalBus      _SignalBus,
		StoreProcessor _StoreProcessor 
	)
	{
		m_SignalBus      = _SignalBus;
		m_StoreProcessor = _StoreProcessor;
	}

	public void Setup(MainMenuPageType _PageType)
	{
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == _PageType)
				page.Show();
			else
				page.Hide();
		}
		
		m_Control.Select(_PageType);
	}

	void IInitializable.Initialize()
	{
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == MainMenuPageType.Levels)
				page.Show(true);
			else
				page.Hide(true);
		}
		
		m_Control.Select(MainMenuPageType.Levels, true);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<PurchaseSignal>(Refresh);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_SignalBus.Unsubscribe<PurchaseSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		m_ProductPromo.Setup(m_StoreProcessor.GetPromoProductID());
	}
}
