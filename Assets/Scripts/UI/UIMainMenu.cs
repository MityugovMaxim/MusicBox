using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Menu(MenuType.MainMenu)]
public class UIMainMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIMainMenuItem m_Item;
	[SerializeField] RectTransform  m_Container;
	[SerializeField] UIProductPromo m_ProductPromo;

	SignalBus              m_SignalBus;
	LevelProcessor         m_LevelProcessor;
	SocialProcessor        m_SocialProcessor;
	MenuProcessor          m_MenuProcessor;
	ConfigProcessor        m_ConfigProcessor;
	UIMainMenuItem.Factory m_ItemFactory;

	readonly List<UIMainMenuItem> m_Items = new List<UIMainMenuItem>();

	List<string> m_LevelIDs;

	[Inject]
	public void Construct(
		SignalBus              _SignalBus,
		LevelProcessor         _LevelProcessor,
		SocialProcessor        _SocialProcessor,
		MenuProcessor          _MenuProcessor,
		ConfigProcessor        _ConfigProcessor,
		UIMainMenuItem.Factory _ItemFactory
	)
	{
		m_SignalBus       = _SignalBus;
		m_LevelProcessor  = _LevelProcessor;
		m_SocialProcessor = _SocialProcessor;
		m_MenuProcessor   = _MenuProcessor;
		m_ConfigProcessor = _ConfigProcessor;
		m_ItemFactory     = _ItemFactory;
	}

	public void Shop()
	{
		m_MenuProcessor.Show(MenuType.ShopMenu);
	}

	public void Achievements()
	{
		if (m_SocialProcessor != null)
			m_SocialProcessor.ShowAchievements();
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<PurchaseSignal>(RegisterPurchase);
		m_SignalBus.Subscribe<ConfigSignal>(RegisterConfig);
		m_SignalBus.Subscribe<LevelDataUpdateSignal>(RegisterLevelDataUpdate);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(RegisterScoreDataUpdate);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<PurchaseSignal>(RegisterPurchase);
		m_SignalBus.Unsubscribe<ConfigSignal>(RegisterConfig);
		m_SignalBus.Unsubscribe<LevelDataUpdateSignal>(RegisterLevelDataUpdate);
		m_SignalBus.Unsubscribe<ScoreDataUpdateSignal>(RegisterScoreDataUpdate);
	}

	protected override void Awake()
	{
		base.Awake();
		
		Show(true);
	}

	protected override void OnShowStarted()
	{
		Refresh();
	}

	void RegisterPurchase()
	{
		Refresh();
		
		m_ProductPromo.Setup(m_ConfigProcessor.PromoProductID);
	}

	void RegisterConfig()
	{
		m_ProductPromo.Setup(m_ConfigProcessor.PromoProductID);
	}

	void RegisterLevelDataUpdate()
	{
		Refresh();
	}

	void RegisterScoreDataUpdate()
	{
		Refresh();
	}

	void Refresh()
	{
		if (m_LevelProcessor == null)
			return;
		
		m_LevelIDs = m_LevelProcessor.GetLevelIDs();
		
		int delta = m_LevelIDs.Count - m_Items.Count;
		int count = Mathf.Abs(delta);
		
		if (delta > 0)
		{
			for (int i = 0; i < count; i++)
			{
				UIMainMenuItem item = m_ItemFactory.Create(m_Item);
				item.RectTransform.SetParent(m_Container, false);
				m_Items.Add(item);
			}
		}
		else if (delta < 0)
		{
			for (int i = 0; i < count; i++)
			{
				int            index = m_Items.Count - 1;
				UIMainMenuItem item  = m_Items[index];
				Destroy(item.gameObject);
				m_Items.RemoveAt(index);
			}
		}
		
		foreach (UIMainMenuItem item in m_Items)
			item.gameObject.SetActive(false);
		
		for (var i = 0; i < m_LevelIDs.Count; i++)
		{
			UIMainMenuItem item    = m_Items[i];
			string         levelID = m_LevelIDs[i];
			
			item.Setup(levelID);
			
			item.gameObject.SetActive(true);
		}
	}
}
