using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UILevelsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Levels;

	[SerializeField] RectTransform m_Container;

	SignalBus            m_SignalBus;
	LevelProcessor       m_LevelProcessor;
	ProfileProcessor     m_ProfileProcessor;
	ScoreProcessor       m_ScoreProcessor;
	StoreProcessor       m_StoreProcessor;
	UILevelsPageItem.Pool  m_ItemPool;
	UIMainMenuGroup.Pool m_GroupPool;

	List<string> m_LevelIDs = new List<string>();

	readonly List<UILevelsPageItem>  m_Items  = new List<UILevelsPageItem>();
	readonly List<UIMainMenuGroup> m_Groups = new List<UIMainMenuGroup>();

	[Inject]
	public void Construct(
		SignalBus            _SignalBus,
		LevelProcessor       _LevelProcessor,
		ProfileProcessor     _ProfileProcessor,
		ScoreProcessor       _ScoreProcessor,
		StoreProcessor       _StoreProcessor,
		UILevelsPageItem.Pool  _ItemPool,
		UIMainMenuGroup.Pool _GroupPool
	)
	{
		m_SignalBus        = _SignalBus;
		m_LevelProcessor   = _LevelProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_ScoreProcessor   = _ScoreProcessor;
		m_StoreProcessor   = _StoreProcessor;
		m_ItemPool         = _ItemPool;
		m_GroupPool        = _GroupPool;
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<LevelDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_SignalBus.Unsubscribe<LevelDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoreDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UILevelsPageItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		foreach (UIMainMenuGroup group in m_Groups)
			m_GroupPool.Despawn(group);
		m_Groups.Clear();
		
		m_LevelIDs = m_LevelProcessor.GetLevelIDs();
		
		CreateLibrary();
		
		CreateProducts();
		
		CreateLocked();
	}

	void CreateLibrary()
	{
		string[] levelIDs = m_LevelIDs
			.Where(m_ProfileProcessor.IsLevelUnlocked)
			.OrderBy(m_ScoreProcessor.GetRank)
			.ToArray();
		
		CreateItemsGroup("Library", levelIDs);
	}

	void CreateProducts()
	{
		string[] levelIDs = m_LevelIDs
			.Where(m_StoreProcessor.ContainsLevel)
			.Where(m_ProfileProcessor.IsLevelLocked)
			.ToArray();
		
		CreateItemsGroup("Products", levelIDs);
	}

	void CreateLocked()
	{
		Dictionary<int, string[]> groups = m_LevelIDs
			.Where(m_ProfileProcessor.IsLevelLocked)
			.SkipWhile(m_StoreProcessor.ContainsLevel)
			.GroupBy(m_LevelProcessor.GetLevel)
			.OrderBy(_LevelGroup => _LevelGroup.Key)
			.ToDictionary(_LevelGroup => _LevelGroup.Key, _LevelGroup => _LevelGroup.ToArray());
		
		foreach (var entry in groups)
			CreateItemsGroup($"Level: {entry.Key}", entry.Value);
	}

	void CreateItemsGroup(string _Title, IReadOnlyCollection<string> _LevelIDs)
	{
		if (_LevelIDs == null || _LevelIDs.Count == 0)
			return;
		
		List<UILevelsPageItem> items = new List<UILevelsPageItem>();
		foreach (string levelID in _LevelIDs)
		{
			UILevelsPageItem item = m_ItemPool.Spawn();
			
			item.Setup(levelID);
			
			items.Add(item);
			
			m_Items.Add(item);
		}
		
		UIMainMenuGroup group = m_GroupPool.Spawn();
		
		group.Setup(_Title, items);
		
		group.RectTransform.SetParent(m_Container, false);
		
		m_Groups.Add(group);
	}
}