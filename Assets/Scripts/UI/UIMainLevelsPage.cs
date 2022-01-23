using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UIMainLevelsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Levels;

	[SerializeField] RectTransform m_Container;

	SignalBus         m_SignalBus;
	LanguageProcessor m_LanguageProcessor;
	LevelProcessor    m_LevelProcessor;
	ProfileProcessor  m_ProfileProcessor;
	ProductProcessor  m_ProductProcessor;
	UILevelItem.Pool  m_ItemPool;
	UILevelGroup.Pool m_GroupPool;

	List<string> m_LevelIDs = new List<string>();

	readonly List<UILevelItem>  m_Items  = new List<UILevelItem>();
	readonly List<UILevelGroup> m_Groups = new List<UILevelGroup>();

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		LanguageProcessor _LanguageProcessor,
		LevelProcessor    _LevelProcessor,
		ProfileProcessor  _ProfileProcessor,
		ProductProcessor  _ProductProcessor,
		UILevelItem.Pool  _ItemPool,
		UILevelGroup.Pool _GroupPool
	)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_ProfileProcessor  = _ProfileProcessor;
		m_ProductProcessor  = _ProductProcessor;
		m_ItemPool          = _ItemPool;
		m_GroupPool         = _GroupPool;
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<LevelDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<LevelDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoreDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UILevelItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		foreach (UILevelGroup group in m_Groups)
			m_GroupPool.Despawn(group);
		m_Groups.Clear();
		
		m_LevelIDs = m_ProfileProcessor.GetVisibleLevelIDs();
		
		CreateLibrary();
		
		CreateProducts();
		
		CreateLocked();
	}

	void CreateLibrary()
	{
		string[] levelIDs = m_LevelIDs
			.Where(m_ProfileProcessor.HasLevel)
			.ToArray();
		
		CreateItemsGroup(m_LanguageProcessor.Get("TRACKS_LIBRARY"), levelIDs);
	}

	void CreateProducts()
	{
		string[] levelIDs = m_LevelIDs
			.Where(m_ProductProcessor.HasLevel)
			.SkipWhile(m_ProfileProcessor.HasLevel)
			.ToArray();
		
		CreateItemsGroup(m_LanguageProcessor.Get("TRACKS_PRODUCTS"), levelIDs);
	}

	void CreateLocked()
	{
		Dictionary<int, string[]> groups = m_LevelIDs
			.Where(_LevelID => !m_ProfileProcessor.HasLevel(_LevelID))
			//.SkipWhile(m_ProductProcessor.HasLevel)
			.GroupBy(m_LevelProcessor.GetLevel)
			.OrderBy(_LevelGroup => _LevelGroup.Key)
			.ToDictionary(_LevelGroup => _LevelGroup.Key, _LevelGroup => _LevelGroup.ToArray());
		
		foreach (var entry in groups)
			CreateItemsGroup(m_LanguageProcessor.Format("TRACKS_LEVEL", $"<sprite name=level_{entry.Key}>"), entry.Value);
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