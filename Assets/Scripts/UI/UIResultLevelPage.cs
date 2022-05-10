using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine;
using Zenject;

public class UIResultLevelPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Level;

	public override bool Valid => m_SourceLevel < m_MaxLevel && m_SourceDiscs < m_TargetDiscs;

	[SerializeField] UILevelProgress m_LevelProgress;
	[SerializeField] UIGroup         m_ItemsGroup;
	[SerializeField] UIUnitLabel     m_CoinsLabel;
	[SerializeField] UIGroup         m_ContinueGroup;

	[SerializeField, Sound] string m_UnitSound;

	[Inject] ScoreManager          m_ScoreManager;
	[Inject] ProgressProcessor     m_ProgressProcessor;
	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] SoundProcessor        m_SoundProcessor;
	[Inject] HapticProcessor       m_HapticProcessor;
	[Inject] StatisticProcessor    m_StatisticProcessor;
	[Inject] UISongUnlockItem.Pool m_ItemPool;

	readonly List<UISongUnlockItem> m_Items   = new List<UISongUnlockItem>();
	readonly Queue<Func<Task>>      m_Actions = new Queue<Func<Task>>();

	string m_SongID;
	int    m_MinLevel;
	int    m_MaxLevel;
	int    m_SourceDiscs;
	int    m_TargetDiscs;
	int    m_SourceLevel;
	int    m_TargetLevel;
	long   m_Coins;

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
		m_StatisticProcessor.LogResultMenuLevelPageContinueClick(m_SongID);
		
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
			m_Actions.Enqueue(PlayCoins);
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
		int sourceLevel = Mathf.Clamp(m_SourceLevel, m_MinLevel, m_MaxLevel);
		int targetLevel = Mathf.Clamp(sourceLevel + 1, m_MinLevel, m_MaxLevel);
		
		m_CoinsLabel.Value = 0;
		m_CoinsLabel.gameObject.SetActive(false);
		m_Coins = m_ProgressProcessor.GetCoins(targetLevel);
		
		m_ContinueGroup.Hide(true);
		
		foreach (UISongUnlockItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		List<string> songIDs = m_ProgressProcessor.GetSongIDs(targetLevel);
		
		foreach (string songID in songIDs)
		{
			UISongUnlockItem item = m_ItemPool.Spawn(m_ItemsGroup.RectTransform);
			
			item.Setup(songID);
			
			m_Items.Add(item);
		}
	}

	Task SetupProgress(int _Level)
	{
		int   sourceLevel    = Mathf.Clamp(_Level, m_MinLevel, m_MaxLevel);
		int   targetLevel    = Mathf.Clamp(sourceLevel + 1, m_MinLevel, m_MaxLevel);
		float sourceProgress = m_ProgressProcessor.GetProgress(sourceLevel, m_SourceDiscs);
		float targetProgress = m_ProgressProcessor.GetProgress(sourceLevel, m_TargetDiscs);
		
		m_LevelProgress.Setup(sourceLevel, targetLevel, sourceProgress, targetProgress);
		
		m_CoinsLabel.Value = 0;
		m_CoinsLabel.gameObject.SetActive(false);
		
		foreach (UISongUnlockItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		List<string> songIDs = m_ProgressProcessor.GetSongIDs(targetLevel);
		
		foreach (string songID in songIDs)
		{
			UISongUnlockItem item = m_ItemPool.Spawn(m_ItemsGroup.RectTransform);
			
			item.Setup(songID);
			
			m_Items.Add(item);
		}
		
		return Task.FromResult(true);
	}

	Task ShowProgress()
	{
		return m_LevelProgress.ShowAsync();
	}

	Task HideProgress()
	{
		return m_LevelProgress.HideAsync();
	}

	Task PlayCoins()
	{
		const float duration = 1.5f;
		
		m_CoinsLabel.gameObject.SetActive(m_Coins > 0);
		
		if (m_Coins <= 0)
			return Task.FromResult(true);
		
		m_HapticProcessor.Play(Haptic.Type.Selection, 30, duration);
		m_SoundProcessor.Start(m_UnitSound);
		
		return UnityTask.Phase(
			_Phase => m_CoinsLabel.Value = MathUtility.Lerp(0, m_Coins, _Phase),
			duration
		).ContinueWithOnMainThread(_Task => m_SoundProcessor.Stop(m_UnitSound));
	}

	async Task PlayItems()
	{
		foreach (UISongUnlockItem item in m_Items)
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