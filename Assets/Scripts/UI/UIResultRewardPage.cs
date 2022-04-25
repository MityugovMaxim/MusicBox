using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine;
using Zenject;

public class UIResultRewardPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Reward;

	[SerializeField] UICascadeTMPLabel  m_Title;
	[SerializeField] UIDiscProgress[]   m_DiscsProgress;
	[SerializeField] UIDiscs            m_Discs;
	[SerializeField] UIGroup            m_ContinueGroup;
	[SerializeField] UIGroup            m_LoaderGroup;
	[SerializeField] UICascadeUnitLabel m_ScoreLabel;
	[SerializeField] UICascadeUnitLabel m_CoinsLabel;
	[SerializeField] float              m_Duration = 1.5f;

	[SerializeField, Sound] string m_UnitSound;

	[Inject] ScoresProcessor   m_ScoresProcessor;
	[Inject] ScoreManager      m_ScoreManager;
	[Inject] SongsProcessor    m_SongsProcessor;
	[Inject] ProfileProcessor  m_ProfileProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;
	[Inject] SoundProcessor    m_SoundProcessor;
	[Inject] HapticProcessor   m_HapticProcessor;

	string    m_SongID;
	ScoreRank m_SourceRank;
	ScoreRank m_TargetRank;
	int       m_SourceLevel;
	int       m_TargetLevel;
	int       m_SourceAccuracy;
	int       m_TargetAccuracy;
	long      m_SourceScore;
	long      m_TargetScore;
	long      m_Coins;

	readonly Queue<Func<Task>> m_Actions = new Queue<Func<Task>>();

	public override void Setup(string _SongID)
	{
		m_SongID         = _SongID;
		m_SourceRank     = m_ScoresProcessor.GetRank(m_SongID);
		m_SourceAccuracy = m_ScoresProcessor.GetAccuracy(m_SongID);
		m_SourceScore    = m_ScoresProcessor.GetScore(m_SongID);
		m_TargetRank     = m_ScoreManager.GetRank();
		m_TargetAccuracy = m_ScoreManager.GetAccuracy();
		m_TargetScore    = m_ScoreManager.GetScore();
		m_Coins          = m_SongsProcessor.GetPayout(m_SongID, m_SourceRank, m_TargetRank);
		m_SourceLevel    = m_ProfileProcessor.Level;
		m_TargetLevel    = m_ProgressProcessor.GetMaxLevel();
		
		m_ScoreLabel.Value = 0;
		m_CoinsLabel.Value = 0;
		m_ContinueGroup.Hide(true);
		m_LoaderGroup.Hide(true);
		
		ProcessActions();
		
		ProcessTitle();
		
		ProcessDiscs();
		
		ProcessResult();
	}

	public override async void Play()
	{
		while (m_Actions.Count > 0)
			await m_Actions.Dequeue().Invoke();
	}

	public async void Continue()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_ContinueGroup.Hide();
		m_LoaderGroup.Show();
		
		SongFinishRequest request = new SongFinishRequest(
			m_SongID,
			m_ScoreManager.GetScore(),
			m_ScoreManager.GetAccuracy()
		);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			await m_ScoresProcessor.Load();
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			ResultMenuPageType pageType = m_SourceLevel != m_TargetLevel && m_TargetRank > m_SourceRank
				? ResultMenuPageType.Level
				: ResultMenuPageType.Control;
			
			UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
			
			if (resultMenu == null)
				return;
			
			await resultMenu.Select(pageType);
			
			resultMenu.Play(pageType);
		}
		else
		{
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_finish",
				"SONG_FINISH_ERROR_TITLE",
				"SONG_FINISH_ERROR_MESSAGE",
				Continue,
				Skip
			);
		}
	}

	async void Skip()
	{
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		if (resultMenu == null)
			return;
		
		await resultMenu.Select(ResultMenuPageType.Control);
		
		resultMenu.Play(ResultMenuPageType.Control);
	}

	void ProcessActions()
	{
		m_Actions.Clear();
		
		m_Actions.Enqueue(PlayScore);
		
		for (ScoreRank rank = m_SourceRank; rank <= m_TargetRank; rank++)
		{
			if (m_SourceAccuracy >= m_TargetAccuracy)
				continue;
			
			UIDiscProgress discProgress = GetDiscProgress(rank + 1);
			
			if (discProgress == null)
				continue;
			
			float sourceProgress = m_ScoreManager.GetRankProgress(rank, m_SourceAccuracy);
			float targetProgress = m_ScoreManager.GetRankProgress(rank, m_TargetAccuracy);
			
			discProgress.Setup(sourceProgress, targetProgress);
			
			if (Mathf.Approximately(sourceProgress, targetProgress))
				continue;
			
			m_Actions.Enqueue(() => discProgress.ShowAsync(true));
			
			m_Actions.Enqueue(discProgress.ProgressAsync);
			
			if (discProgress.Rank <= m_TargetRank)
				m_Actions.Enqueue(() => CollectDisc(discProgress));
		}
		
		if (m_TargetRank > ScoreRank.None)
			m_Actions.Enqueue(DiscsResult);
		
		m_Actions.Enqueue(PlayCoins);
		m_Actions.Enqueue(ShowControl);
	}

	void ProcessTitle()
	{
		m_Title.Text = m_SourceScore < m_TargetScore
			? GetLocalization("RESULT_NEW_RECORD")
			: GetLocalization("RESULT_REWARD");
	}

	void ProcessDiscs()
	{
		foreach (UIDiscProgress discProgress in m_DiscsProgress)
		{
			if (discProgress != null)
				discProgress.Hide(true);
		}
		
		if (m_SourceRank >= ScoreRank.Platinum)
			return;
		
		if (m_SourceRank >= m_TargetRank && m_SourceAccuracy >= m_TargetAccuracy && m_SourceRank > ScoreRank.None)
			return;
		
		UIDiscProgress source = GetDiscProgress(m_SourceRank + 1);
		
		if (source == null)
			return;
		
		source.Setup(
			m_ScoreManager.GetRankProgress(m_SourceRank, m_SourceAccuracy),
			m_ScoreManager.GetRankProgress(m_SourceRank, m_TargetAccuracy)
		);
		
		source.Show(true);
	}

	void ProcessResult()
	{
		m_Discs.Rank = m_TargetRank >= m_SourceRank ? m_TargetRank : m_SourceRank;
		
		if (m_SourceRank >= ScoreRank.Platinum || m_TargetRank >= ScoreRank.None && m_SourceAccuracy >= m_TargetAccuracy)
			m_Discs.Show(true);
		else
			m_Discs.Hide(true);
	}

	UIDiscProgress GetDiscProgress(ScoreRank _Rank)
	{
		return m_DiscsProgress.FirstOrDefault(_DiscProgress => _DiscProgress.Rank == _Rank);
	}

	Task CollectDisc(UIDiscProgress _DiscProgress)
	{
		return Task.WhenAny(
			_DiscProgress.CollectAsync().ContinueWithOnMainThread(_Task => _DiscProgress.HideAsync()),
			Task.Delay(250)
		);
	}

	Task PlayScore()
	{
		return Task.WhenAll(
			m_Title.PlayAsync(),
			UnitAsync(m_ScoreLabel, m_TargetScore)
		);
	}

	Task DiscsResult()
	{
		List<Task> tasks = new List<Task>();
		foreach (UIDiscProgress discProgress in m_DiscsProgress)
		{
			if (discProgress != null)
				tasks.Add(discProgress.HideAsync());
		}
		tasks.Add(m_Discs.ShowAsync());
		
		return Task.WhenAll(tasks);
	}

	Task PlayCoins()
	{
		return UnitAsync(m_CoinsLabel, m_Coins);
	}

	Task ShowControl()
	{
		return m_ContinueGroup.ShowAsync();
	}

	Task UnitAsync(UICascadeUnitLabel _Label, double _Value, CancellationToken _Token = default)
	{
		m_HapticProcessor.Play(Haptic.Type.Selection, 30, m_Duration);
		m_SoundProcessor.Start(m_UnitSound);
		
		long value = (long)_Value;
		
		if (Math.Abs(value) <= 1)
		{
			_Label.Value = value;
			return Task.FromResult(true);
		}
		
		return UnityTask.Phase(
			_Phase => _Label.Value = MathUtility.Lerp(0, value, _Phase),
			m_Duration,
			_Token
		).ContinueWithOnMainThread(
			_Task =>
			{
				m_SoundProcessor.Stop(m_UnitSound);
				
				_Label.Play();
			},
			_Token
		);
	}
}
