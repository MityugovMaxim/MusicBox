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

	[Inject] ScoreManager          m_ScoreManager;
	[Inject] ProgressProcessor     m_ProgressProcessor;
	[Inject] SongsManager          m_SongsManager;
	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] StatisticProcessor    m_StatisticProcessor;
	[Inject] UISongUnlockItem.Pool m_ItemPool;

	readonly Queue<ProgressData>    m_ProgressData = new Queue<ProgressData>();
	readonly List<UISongUnlockItem> m_Items        = new List<UISongUnlockItem>();

	string m_SongID;
	int    m_SourceDiscs;
	int    m_TargetDiscs;
	int    m_MinLevel;
	int    m_MaxLevel;
	int    m_SourceLevel;
	int    m_TargetLevel;

	public override void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_MinLevel    = m_ProgressProcessor.GetMinLevel();
		m_MaxLevel    = m_ProgressProcessor.GetMaxLevel();
		m_SourceDiscs = m_ScoreManager.GetSourceDiscs();
		m_TargetDiscs = m_ScoreManager.GetTargetDiscs();
		m_SourceLevel = Mathf.Clamp(m_ProgressProcessor.GetLevel(m_SourceDiscs), m_MinLevel, m_MaxLevel);
		m_TargetLevel = Mathf.Clamp(m_ProgressProcessor.GetLevel(m_TargetDiscs), m_MinLevel, m_MaxLevel);
		
		ProcessProgress();
		
		m_LevelProgress.Hide(true);
		m_ItemsGroup.Hide(true);
		m_ContinueGroup.Hide(true);
	}

	void ProcessProgress()
	{
		m_ProgressData.Clear();
		
		for (int level = m_SourceLevel; level <= m_TargetLevel; level++)
		{
			if (level >= m_MaxLevel)
				break;
			
			int minLimit = m_ProgressProcessor.GetMinLimit(level);
			int maxLimit = m_ProgressProcessor.GetMaxLimit(level);
			
			m_ProgressData.Enqueue(
				new ProgressData(
					level,
					Mathf.InverseLerp(minLimit, maxLimit, m_SourceDiscs),
					Mathf.InverseLerp(minLimit, maxLimit, m_TargetDiscs)
				)
			);
		}
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
			
			ProcessItems(progressData.Level + 1);
			
			await Task.WhenAll(
				m_LevelProgress.ShowAsync(),
				m_ItemsGroup.ShowAsync()
			);
			
			await m_LevelProgress.ProgressAsync();
			
			if (m_ProgressData.Count == 0)
				break;
			
			await m_LevelProgress.CollectAsync();
			
			await UnlockItems();
			
			await Task.Delay(1500);
			
			await Task.WhenAll(
				m_LevelProgress.HideAsync(),
				m_ItemsGroup.HideAsync()
			);
		}
		
		m_ContinueGroup.Show();
	}

	void ProcessItems(int _Level)
	{
		foreach (UISongUnlockItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		List<string> songIDs = m_SongsManager.GetLockedSongIDs(_Level);
		
		foreach (string songID in songIDs)
		{
			UISongUnlockItem item = m_ItemPool.Spawn(m_ItemsGroup.RectTransform);
			
			item.Setup(songID);
			
			m_Items.Add(item);
		}
	}

	public async void Continue()
	{
		m_StatisticProcessor.LogResultMenuLevelPageContinueClick(m_SongID);
		
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