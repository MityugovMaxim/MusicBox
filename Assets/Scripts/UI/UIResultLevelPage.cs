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

	[Inject] ProfileProcessor      m_ProfileProcessor;
	[Inject] ScoreProcessor        m_ScoreProcessor;
	[Inject] ProgressProcessor     m_ProgressProcessor;
	[Inject] SongsManager          m_SongsManager;
	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] StatisticProcessor    m_StatisticProcessor;
	[Inject] UISongUnlockItem.Pool m_ItemPool;

	readonly Queue<ProgressData>    m_ProgressData = new Queue<ProgressData>();
	readonly List<UISongUnlockItem> m_Items        = new List<UISongUnlockItem>();

	string m_LevelID;

	public override void Setup(string _SongID)
	{
		m_LevelID = _SongID;
		
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
		m_ContinueGroup.Hide(true);
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
		foreach (UISongUnlockItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
	}

	void CreateItems(int _Level)
	{
		List<string> levelIDs = m_SongsManager.GetLockedSongIDs(_Level);
		
		foreach (string levelID in levelIDs)
		{
			UISongUnlockItem songItem = m_ItemPool.Spawn();
			
			// TODO: Fix
			//item.Setup(m_StorageProcessor.LoadLevelThumbnail(levelID));
			
			songItem.RectTransform.SetParent(m_ItemsGroup.RectTransform, false);
			
			m_Items.Add(songItem);
		}
	}

	public async void Continue()
	{
		m_StatisticProcessor.LogResultMenuLevelPageContinueClick(m_LevelID);
		
		m_ContinueGroup.Hide();
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		if (resultMenu == null)
			return;
		
		await resultMenu.Select(ResultMenuPageType.Control);
		
		resultMenu.Play(ResultMenuPageType.Control);
	}

	async Task UnlockItems()
	{
		foreach (UISongUnlockItem item in m_Items)
		{
			await Task.WhenAny(
				item.PlayAsync(),
				Task.Delay(250)
			);
		}
	}
}