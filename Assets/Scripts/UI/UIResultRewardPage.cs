using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine;
using Zenject;

public class UIResultRewardPage : UIResultMenuPage
{
	class ProgressData
	{
		public ScoreRank Rank   { get; }
		public float     Source { get; }
		public float     Target { get; }

		public ProgressData(ScoreRank _Rank, float _Source, float _Target)
		{
			Rank   = _Rank;
			Source = _Source;
			Target = _Target;
		}
	}

	public override ResultMenuPageType Type => ResultMenuPageType.Reward;

	[SerializeField] UICascadeTMPLabel  m_Title;
	[SerializeField] UIDiscProgress[]   m_DiscsProgress;
	[SerializeField] UIDiscs            m_Discs;
	[SerializeField] UIGroup            m_ContinueGroup;
	[SerializeField] UIGroup            m_LoaderGroup;
	[SerializeField] UICascadeUnitLabel m_ScoreLabel;
	[SerializeField] UICascadeUnitLabel m_CoinsLabel;
	[SerializeField] float              m_Duration = 1.5f;
	[SerializeField] AnimationCurve     m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);

	[SerializeField, Sound] string m_UnitSound;

	[Inject] ScoresProcessor    m_ScoresProcessor;
	[Inject] ScoreManager       m_ScoreManager;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] ProgressProcessor  m_ProgressProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] SoundProcessor     m_SoundProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string      m_SongID;
	ScoreRank   m_SourceRank;
	ScoreRank   m_TargetRank;
	int         m_SourceLevel;
	int         m_TargetLevel;
	int         m_SourceAccuracy;
	int         m_TargetAccuracy;
	long        m_SourceScore;
	long        m_TargetScore;
	long        m_Coins;
	IEnumerator m_ScoreRoutine;
	IEnumerator m_CoinsRoutine;

	readonly Queue<ProgressData> m_ProgressData = new Queue<ProgressData>();

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
		m_SourceLevel    = m_ProgressProcessor.ClampLevel(m_ProfileProcessor.Level);
		m_TargetLevel    = m_ProgressProcessor.GetMaxLevel();
		
		ProcessProgress();
		
		ProcessDiscs();
		
		ProcessTitle();
		
		m_ScoreLabel.Value = 0;
		m_CoinsLabel.Value = 0;
		m_ContinueGroup.Hide(true);
		m_LoaderGroup.Hide(true);
	}

	public override async void Play()
	{
		await PlayTitle();
		
		await PlayScore();
		
		await Task.Delay(500);
		
		await PlayRank();
		
		await Task.Delay(500);
		
		await PlayCoins();
		
		await Task.Delay(500);
		
		m_ContinueGroup.Show();
	}

	public async void Continue()
	{
		m_StatisticProcessor.LogResultMenuRewardPageContinueClick(m_SongID);
		
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
			
			Skip();
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
		
		ResultMenuPageType pageType = m_SourceLevel != m_TargetLevel && m_TargetRank > m_SourceRank
			? ResultMenuPageType.Level
			: ResultMenuPageType.Control;
		
		await resultMenu.Select(pageType);
		
		resultMenu.Play(pageType);
	}

	void ProcessTitle()
	{
		m_Title.Text = m_SourceScore > m_TargetScore
			? GetLocalization("RESULT_NEW_RECORD")
			: GetLocalization("RESULT_TITLE");
	}

	void ProcessProgress()
	{
		m_ProgressData.Clear();
		for (ScoreRank rank = m_SourceRank; rank <= m_TargetRank; rank++)
		{
			if (rank >= ScoreRank.Platinum)
				break;
			
			float sourceProgress = m_ScoreManager.GetRankSourceProgress(rank);
			float targetProgress = m_ScoreManager.GetRankTargetProgress(rank);
			
			m_ProgressData.Enqueue(new ProgressData(rank, sourceProgress, targetProgress));
		}
	}

	void ProcessDiscs()
	{
		foreach (UIDiscProgress discProgress in m_DiscsProgress)
		{
			if (discProgress.Rank != m_SourceRank + 1)
			{
				discProgress.Hide(true);
				continue;
			}
			
			float progress = m_ScoreManager.GetRankSourceProgress(m_SourceRank);
			
			discProgress.Setup(progress, progress);
			discProgress.Show(true);
		}
		
		m_Discs.Rank = m_TargetRank > m_SourceRank ? m_TargetRank : m_SourceRank;
		
		if (m_TargetAccuracy > m_SourceAccuracy && m_SourceRank < ScoreRank.Platinum)
			m_Discs.Hide(true);
		else
			m_Discs.Show(true);
	}

	UIDiscProgress GetDiscProgress(ScoreRank _Rank)
	{
		return m_DiscsProgress.FirstOrDefault(_DiscProgress => _DiscProgress.Rank == _Rank);
	}

	Task PlayTitle()
	{
		return m_Title.PlayAsync();
	}

	async Task PlayRank()
	{
		while (m_ProgressData.Count > 0)
		{
			ProgressData progressData = m_ProgressData.Dequeue();
			
			UIDiscProgress discProgress = GetDiscProgress(progressData.Rank + 1);
			
			if (discProgress == null)
				continue;
			
			discProgress.Setup(progressData.Source, progressData.Target);
			
			await discProgress.ShowAsync();
			
			await discProgress.ProgressAsync();
			
			if (m_TargetRank == ScoreRank.None)
				return;
			
			if (m_TargetRank <= progressData.Rank)
			{
				await Task.WhenAny(
					discProgress.HideAsync(),
					Task.Delay(250)
				);
			}
			else
			{
				await Task.WhenAny(
					discProgress.CollectAsync().ContinueWith(_Task => discProgress.HideAsync()),
					Task.Delay(250)
				);
			}
		}
		
		if (m_SourceRank > ScoreRank.None || m_TargetRank > ScoreRank.None)
			await m_Discs.ShowAsync();
	}

	Task PlayScore(CancellationToken _Token = default)
	{
		return UnitAsync(m_ScoreLabel, m_TargetScore, _Token);
	}

	Task PlayCoins(CancellationToken _Token = default)
	{
		return UnitAsync(m_CoinsLabel, m_Coins, _Token);
	}

	Task UnitAsync(UICascadeUnitLabel _Label, double _Value, CancellationToken _Token = default)
	{
		m_HapticProcessor.Play(Haptic.Type.Selection, 30, m_Duration);
		m_SoundProcessor.Start(m_UnitSound);
		
		Task task = UnityTask.Phase(
			_Phase => _Label.Value = (long)(_Value * m_Curve.Evaluate(_Phase)),
			m_Duration,
			_Token
		);
		
		task = task.ContinueWithOnMainThread(
			_Task =>
			{
				m_SoundProcessor.Stop(m_UnitSound);
				_Label.Play();
			},
			_Token
		);
		
		return task;
	}
}
