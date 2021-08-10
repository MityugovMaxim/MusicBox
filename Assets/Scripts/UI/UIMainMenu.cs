using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIMainMenuItem m_Item;
	[SerializeField] RectTransform  m_Container;
	//[SerializeField] ScrollRect     m_Scroll;

	SignalBus              m_SignalBus;
	LevelProcessor         m_LevelProcessor;
	SocialProcessor        m_SocialProcessor;
	MenuProcessor          m_MenuProcessor;
	UIMainMenuItem.Factory m_ItemFactory;

	readonly List<UIMainMenuItem> m_Items = new List<UIMainMenuItem>();

	string[] m_LevelIDs;

	[Inject]
	public void Construct(
		SignalBus              _SignalBus,
		LevelProcessor         _LevelProcessor,
		SocialProcessor        _SocialProcessor,
		MenuProcessor          _MenuProcessor,
		UIMainMenuItem.Factory _ItemFactory
	)
	{
		m_SignalBus       = _SignalBus;
		m_LevelProcessor  = _LevelProcessor;
		m_SocialProcessor = _SocialProcessor;
		m_MenuProcessor   = _MenuProcessor;
		m_ItemFactory     = _ItemFactory;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<PurchaseSignal>(RegisterPurchase);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<PurchaseSignal>(RegisterPurchase);
	}

	void RegisterPurchase()
	{
		Refresh();
	}

	protected override void Awake()
	{
		base.Awake();
		
		Show(true);
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

	protected override void OnShowStarted()
	{
		Refresh();
	}

	void Refresh()
	{
		if (m_LevelProcessor == null)
			return;
		
		m_LevelIDs = m_LevelProcessor.GetLevelIDs();
		
		int delta = m_LevelIDs.Length - m_Items.Count;
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
		
		for (var i = 0; i < m_LevelIDs.Length; i++)
		{
			UIMainMenuItem item    = m_Items[i];
			string         levelID = m_LevelIDs[i];
			
			item.Setup(levelID);
			
			item.gameObject.SetActive(true);
		}
	}

	// void Recenter(string _LevelID)
	// {
	// 	if (string.IsNullOrEmpty(_LevelID))
	// 	{
	// 		Debug.LogErrorFormat("[UIMainMenu] Recenter failed. Level ID '{0}' is null or empty.", _LevelID);
	// 		return;
	// 	}
	// 	
	// 	UIMainMenuItem item = m_Items.FirstOrDefault(_Track => _Track.gameObject.activeInHierarchy && _Track.LevelID == _LevelID);
	// 	
	// 	if (item == null)
	// 	{
	// 		Debug.LogErrorFormat("[UIMainMenu] Recenter failed. Track with level ID '{0}' not found.", _LevelID);
	// 		return;
	// 	}
	// 	
	// 	Rect source = item.GetWorldRect();
	// 	Rect target = m_Scroll.content.GetWorldRect();
	// 	
	// 	float position = MathUtility.Remap01(source.yMin, target.yMin, target.yMax - source.height);
	// 	
	// 	m_Scroll.StopMovement();
	// 	m_Scroll.verticalNormalizedPosition = position;
	// }
}
