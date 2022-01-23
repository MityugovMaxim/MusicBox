using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultLevelPage : UIResultMenuPage
{
	class ProgressData
	{
		public int   Level          { get; }
		public float Source { get; }
		public float Target { get; }

		public ProgressData(int _Level, float _Source, float _Target)
		{
			Level  = _Level;
			Source = _Source;
			Target = _Target;
		}
	}

	public override ResultMenuPageType Type => ResultMenuPageType.Level;

	[SerializeField] UILevelProgress m_LevelProgress;
	[SerializeField] UIGroup         m_ItemsGroup;
	[SerializeField] UIGroup         m_ContinueGroup;

	ScoreProcessor    m_ScoreProcessor;
	ProfileProcessor  m_ProfileProcessor;
	ProgressProcessor m_ProgressProcessor;
	LevelProcessor    m_LevelProcessor;
	StorageProcessor  m_StorageProcessor;
	MenuProcessor     m_MenuProcessor;
	HapticProcessor   m_HapticProcessor;
	UIUnlockItem.Pool m_ItemPool;

	readonly Queue<ProgressData> m_ProgressData = new Queue<ProgressData>();
	readonly List<string>        m_LevelIDs     = new List<string>();
	readonly List<UIUnlockItem>  m_Items        = new List<UIUnlockItem>();

	[Inject]
	public void Construct(
		ScoreProcessor    _ScoreProcessor,
		ProfileProcessor  _ProfileProcessor,
		ProgressProcessor _ProgressProcessor,
		LevelProcessor    _LevelProcessor,
		StorageProcessor  _StorageProcessor,
		MenuProcessor     _MenuProcessor,
		HapticProcessor   _HapticProcessor,
		UIUnlockItem.Pool _ItemPool
	)
	{
		m_ScoreProcessor    = _ScoreProcessor;
		m_ProfileProcessor  = _ProfileProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_StorageProcessor  = _StorageProcessor;
		m_MenuProcessor     = _MenuProcessor;
		m_HapticProcessor   = _HapticProcessor;
		m_ItemPool          = _ItemPool;
	}

	public override void Setup(string _LevelID)
	{
		int sourceDiscs = m_ProfileProcessor.Discs;
		int targetDiscs = m_ProfileProcessor.Discs + (int)m_ScoreProcessor.Rank;
		int minLevel    = m_ProgressProcessor.GetMinLevel();
		int maxLevel    = m_ProgressProcessor.GetMaxLevel();
		int sourceLevel = Mathf.Clamp(m_ProgressProcessor.GetLevel(sourceDiscs), minLevel, maxLevel);
		int targetLevel = Mathf.Clamp(m_ProgressProcessor.GetLevel(targetDiscs), minLevel, maxLevel);
		
		m_ProgressData.Clear();
		for (int level = sourceLevel; level <= targetLevel; level++)
		{
			if (level >= maxLevel)
				break;
			
			int minLimit = m_ProgressProcessor.GetMinLimit(level);
			int maxLimit = m_ProgressProcessor.GetMaxLimit(level);
			
			m_ProgressData.Enqueue(
				new ProgressData(
					level,
					Mathf.InverseLerp(minLimit, maxLimit, sourceDiscs),
					Mathf.InverseLerp(minLimit, maxLimit, targetDiscs)
				)
			);
		}
		
		m_LevelProgress.Hide(true);
		m_ItemsGroup.Hide(true);
	}

	public override async void Play()
	{
		while (m_ProgressData.Count > 0)
		{
			ProgressData progressData = m_ProgressData.Dequeue();
			
			m_LevelProgress.Setup(
				progressData.Level,
				progressData.Level + 1,
				progressData.Source,
				progressData.Target
			);
			
			CreateItems(progressData.Level + 2);
			
			await Task.WhenAll(
				m_LevelProgress.ShowAsync(),
				m_ItemsGroup.ShowAsync()
			);
			
			await m_LevelProgress.Progress();
			
			if (m_ProgressData.Count == 0)
				break;
			
			await m_LevelProgress.Collect();
			
			await UnlockItems();
			
			await Task.Delay(1500);
			
			await Task.WhenAll(
				m_LevelProgress.HideAsync(),
				m_ItemsGroup.HideAsync()
			);
			
			RemoveItems();
		}
		
		m_ContinueGroup.Show();
	}

	void RemoveItems()
	{
		foreach (UIUnlockItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
	}

	void CreateItems(int _Level)
	{
		m_LevelIDs.Clear();
		foreach (string levelID in m_ProfileProcessor.GetVisibleLevelIDs())
		{
			if (_Level == m_LevelProcessor.GetLevel(levelID))
				m_LevelIDs.Add(levelID);
		}
		
		foreach (string levelID in m_LevelIDs)
		{
			UIUnlockItem item = m_ItemPool.Spawn();
			
			item.Setup(m_StorageProcessor.LoadLevelThumbnail(levelID));
			
			item.RectTransform.SetParent(m_ItemsGroup.RectTransform, false);
			
			m_Items.Add(item);
		}
	}

	public async void Continue()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_ContinueGroup.Hide();
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		if (resultMenu == null)
			return;
		
		await resultMenu.Select(ResultMenuPageType.Control);
		
		resultMenu.Play(ResultMenuPageType.Control);
	}

	async Task UnlockItems()
	{
		foreach (UIUnlockItem item in m_Items)
		{
			await Task.WhenAny(
				item.PlayAsync(),
				Task.Delay(250)
			);
		}
	}
}