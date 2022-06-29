using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultLevelPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Level;

	public override bool Valid => m_SourceLevel < m_MaxLevel && m_SourceDiscs < m_TargetDiscs;

	[SerializeField] UILevelProgress m_LevelProgress;
	[SerializeField] UIGroup         m_ItemsGroup;
	[SerializeField] UIGroup         m_ContinueGroup;

	[Inject] ScoreManager      m_ScoreManager;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;
	[Inject] UIUnlockItem.Pool m_ItemPool;

	readonly List<UIUnlockItem> m_Items   = new List<UIUnlockItem>();
	readonly Queue<Func<Task>>  m_Actions = new Queue<Func<Task>>();

	string m_SongID;
	int    m_MinLevel;
	int    m_MaxLevel;
	int    m_SourceDiscs;
	int    m_TargetDiscs;
	int    m_SourceLevel;
	int    m_TargetLevel;

	public override void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_MinLevel    = m_ProgressProcessor.GetMinLevel();
		m_MaxLevel    = m_ProgressProcessor.GetMaxLevel();
		m_SourceDiscs = m_ScoreManager.GetSourceDiscs();
		m_TargetDiscs = m_ScoreManager.GetTargetDiscs();
		m_SourceLevel = m_ProgressProcessor.GetLevel(m_SourceDiscs);
		m_TargetLevel = m_ProgressProcessor.GetLevel(m_TargetDiscs);
		
		m_LevelProgress.Hide(true);
		m_ItemsGroup.Hide(true);
		m_ContinueGroup.Hide(true);
		
		ProcessActions();
		
		ProcessProgress();
	}

	public override async void Play()
	{
		while (m_Actions.Count > 0)
			await m_Actions.Dequeue().Invoke();
	}

	public void Continue()
	{
		m_ContinueGroup.Hide();
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		resultMenu.Next();
	}

	void ProcessActions()
	{
		m_Actions.Clear();
		
		for (int level = m_SourceLevel; level <= m_TargetLevel; level++)
		{
			int levelClosure = level;
			
			m_Actions.Enqueue(() => SetupProgress(levelClosure));
			m_Actions.Enqueue(ShowProgress);
			m_Actions.Enqueue(() => m_ItemsGroup.ShowAsync());
			m_Actions.Enqueue(m_LevelProgress.ProgressAsync);
			
			if (level >= m_TargetLevel)
				break;
			
			m_Actions.Enqueue(m_LevelProgress.CollectAsync);
			m_Actions.Enqueue(() => Task.Delay(1000));
			m_Actions.Enqueue(PlayItems);
			m_Actions.Enqueue(() => Task.Delay(1000));
			
			if (level + 1 >= m_MaxLevel)
				break;
			
			m_Actions.Enqueue(() => m_ItemsGroup.HideAsync());
			m_Actions.Enqueue(HideProgress);
		}
		
		m_Actions.Enqueue(ShowControl);
	}

	void ProcessProgress()
	{
		m_ContinueGroup.Hide(true);
		
		foreach (UIUnlockItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
	}

	Task SetupProgress(int _Level)
	{
		int   sourceLevel    = Mathf.Clamp(_Level, m_MinLevel, m_MaxLevel);
		int   targetLevel    = Mathf.Clamp(sourceLevel + 1, m_MinLevel, m_MaxLevel);
		float sourceProgress = m_ProgressProcessor.GetProgress(sourceLevel, m_SourceDiscs);
		float targetProgress = m_ProgressProcessor.GetProgress(sourceLevel, m_TargetDiscs);
		
		m_LevelProgress.Setup(sourceLevel, targetLevel, sourceProgress, targetProgress);
		
		foreach (UIUnlockItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		CreateCoins(targetLevel);
		
		CreateSongs(targetLevel);
		
		return Task.CompletedTask;
	}

	void CreateCoins(int _Level)
	{
		long coins = m_ProgressProcessor.GetCoins(_Level);
		
		UIUnlockItem item = m_ItemPool.Spawn(m_ItemsGroup.RectTransform);
		
		item.Setup("Thumbnails/Coins/coins_1.jpg", null, coins);
		
		m_Items.Add(item);
	}

	void CreateSongs(int _Level)
	{
		List<string> songIDs = m_ProgressProcessor.GetSongIDs(_Level);
		
		foreach (string songID in songIDs)
		{
			UIUnlockItem item = m_ItemPool.Spawn(m_ItemsGroup.RectTransform);
			
			item.Setup($"Thumbnails/Songs/{songID}.jpg");
			
			m_Items.Add(item);
		}
	}

	Task ShowProgress()
	{
		return m_LevelProgress.ShowAsync();
	}

	Task HideProgress()
	{
		return m_LevelProgress.HideAsync();
	}

	async Task PlayItems()
	{
		foreach (UIUnlockItem item in m_Items)
		{
			await Task.WhenAny(
				item.PlayAsync(),
				Task.Delay(250)
			);
		}
	}

	Task ShowControl()
	{
		return m_ContinueGroup.ShowAsync();
	}
}