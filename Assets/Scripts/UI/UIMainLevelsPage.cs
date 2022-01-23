using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainLevelsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Levels;

	[SerializeField] RectTransform m_Container;

	SignalBus         m_SignalBus;
	LevelManager      m_LevelManager;
	LanguageProcessor m_LanguageProcessor;
	UILevelItem.Pool  m_ItemPool;
	UILevelGroup.Pool m_GroupPool;

	readonly List<UILevelItem>  m_Items  = new List<UILevelItem>();
	readonly List<UILevelGroup> m_Groups = new List<UILevelGroup>();

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		LevelManager      _LevelManager,
		LanguageProcessor _LanguageProcessor,
		UILevelItem.Pool  _ItemPool,
		UILevelGroup.Pool _GroupPool
	)
	{
		m_SignalBus         = _SignalBus;
		m_LevelManager      = _LevelManager;
		m_LanguageProcessor = _LanguageProcessor;
		m_ItemPool          = _ItemPool;
		m_GroupPool         = _GroupPool;
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<LevelDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<LevelDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoreDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UILevelItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		foreach (UILevelGroup group in m_Groups)
			m_GroupPool.Despawn(group);
		m_Groups.Clear();
		
		CreateLibrary();
		
		CreateProducts();
		
		CreateLocked();
	}

	void CreateLibrary()
	{
		List<string> levelIDs = m_LevelManager.GetLibraryLevelIDs();
		
		CreateItemsGroup(m_LanguageProcessor.Get("TRACKS_LIBRARY"), levelIDs);
	}

	void CreateProducts()
	{
		List<string> levelIDs = m_LevelManager.GetProductLevelIDs();
		
		CreateItemsGroup(m_LanguageProcessor.Get("TRACKS_PRODUCTS"), levelIDs);
	}

	void CreateLocked()
	{
		Dictionary<int, string[]> levelsGroups = m_LevelManager.GetLockedLevelIDs();
		
		foreach (var levelsGroup in levelsGroups)
			CreateItemsGroup(m_LanguageProcessor.Format("TRACKS_LEVEL", $"<sprite name=level_{levelsGroup.Key}>"), levelsGroup.Value);
	}

	void CreateItemsGroup(string _Title, IReadOnlyCollection<string> _LevelIDs)
	{
		if (_LevelIDs == null || _LevelIDs.Count == 0)
			return;
		
		List<UILevelItem> items = new List<UILevelItem>();
		foreach (string levelID in _LevelIDs)
		{
			UILevelItem item = m_ItemPool.Spawn();
			
			item.Setup(levelID);
			
			items.Add(item);
			
			m_Items.Add(item);
		}
		
		UILevelGroup group = m_GroupPool.Spawn();
		
		group.Setup(_Title, items);
		
		group.RectTransform.SetParent(m_Container, false);
		
		m_Groups.Add(group);
	}
}