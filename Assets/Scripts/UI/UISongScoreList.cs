using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UISongScoreList : UIEntity
{
	[SerializeField] UISongImage          m_Image;
	[SerializeField] UISongLabel          m_Label;
	[SerializeField] UIUnitLabel          m_Score;
	[SerializeField] UIDisc               m_Disc;
	[SerializeField] UIFlare              m_Flare;
	[SerializeField] UISongScoreCoins     m_Coins;
	[SerializeField] UISongStatistics     m_Statistics;
	[SerializeField] UISongScoreMilestone m_Milestone; 
	[SerializeField] UISongScoreElement[] m_UpperElements;
	[SerializeField] UISongScoreElement[] m_LowerElements;
	[SerializeField] AnimationCurve       m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] float                m_Duration = 3;

	[SerializeField, Sound] string m_MoveSound;
	[SerializeField, Sound] string m_PlatinumSound;
	[SerializeField, Sound] string m_GoldSound;
	[SerializeField, Sound] string m_SilverSound;
	[SerializeField, Sound] string m_BronzeSound;

	[Inject] ScoresManager   m_ScoresManager;
	[Inject] ScoreController m_ScoreController;
	[Inject] SongsManager    m_SongsManager;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	CancellationTokenSource m_DiscToken;
	ScoreRank               m_CoinsRank;
	ScoreRank               m_DiscRank;

	string m_SongID;

	int m_SourceAccuracy;
	int m_TargetAccuracy;

	int m_BronzeThreshold;
	int m_SilverThreshold;
	int m_GoldThreshold;
	int m_PlatinumThreshold;

	public void Setup(string _SongID)
	{
		m_SongID         = _SongID;
		m_SourceAccuracy = m_ScoresManager.GetAccuracy(m_SongID);
		m_TargetAccuracy = m_ScoreController.GetAccuracy();
		
		m_BronzeThreshold   = m_SongsManager.GetThreshold(m_SongID, ScoreRank.Bronze);
		m_SilverThreshold   = m_SongsManager.GetThreshold(m_SongID, ScoreRank.Silver);
		m_GoldThreshold     = m_SongsManager.GetThreshold(m_SongID, ScoreRank.Gold);
		m_PlatinumThreshold = m_SongsManager.GetThreshold(m_SongID, ScoreRank.Platinum);
		
		m_Image.SongID = m_SongID;
		m_Label.SongID = m_SongID;
		
		m_Milestone.Setup(m_SongID);
		
		m_Score.Value = 0;
		m_Coins.Value = 0;
		
		m_DiscRank  = m_ScoresManager.GetRank(m_SongID);
		m_CoinsRank = ScoreRank.None;
		
		m_Disc.Rank = m_DiscRank;
		
		SetAccuracy(0);
		
		ProcessPhase(0);
	}

	public async Task PlayAsync()
	{
		await Task.Delay(1000);
		
		await m_Statistics.PlayAsync();
		
		await Task.WhenAll(
			ScoreAsync(m_ScoreController.GetScore()),
			ShiftAsync()
		);
		
		await Task.Delay(500);
		
		m_Coins.Next();
		
		await Task.Delay(3000);
		
		m_Coins.Next();
		
		await Task.Delay(500);
	}

	public async Task DoubleAsync()
	{
		m_Coins.Value *= 2;
		
		await Task.Delay(2000);
	}

	ScoreRank GetDisc(int _Accuracy)
	{
		if (_Accuracy == m_PlatinumThreshold)
			return ScoreRank.Platinum;
		else if (_Accuracy == m_GoldThreshold)
			return ScoreRank.Gold;
		else if (_Accuracy == m_SilverThreshold)
			return ScoreRank.Silver;
		else if (_Accuracy == m_BronzeThreshold)
			return ScoreRank.Bronze;
		else
			return ScoreRank.None;
	}

	ScoreRank GetRank(int _Accuracy)
	{
		if (_Accuracy >= m_PlatinumThreshold)
			return ScoreRank.Platinum;
		else if (_Accuracy >= m_GoldThreshold)
			return ScoreRank.Gold;
		else if (_Accuracy >= m_SilverThreshold)
			return ScoreRank.Silver;
		else if (_Accuracy >= m_BronzeThreshold)
			return ScoreRank.Bronze;
		else
			return ScoreRank.None;
	}

	string GetDiscSound(ScoreRank _Rank)
	{
		switch (_Rank)
		{
			case ScoreRank.Platinum: return m_PlatinumSound;
			case ScoreRank.Gold:     return m_GoldSound;
			case ScoreRank.Silver:   return m_SilverSound;
			case ScoreRank.Bronze:   return m_BronzeSound;
			default:                 return string.Empty;
		}
	}

	Task ScoreAsync(long _Score)
	{
		long source = (long)m_Score.Value;
		long target = _Score;
		
		return UnityTask.Phase(
			_Phase => m_Score.Value = MathUtility.Lerp(source, target, _Phase),
			m_Duration,
			m_Curve
		);
	}

	Task ShiftAsync()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		IEnumerator routine = ShiftRoutine(() => completionSource.TrySetResult(true));
		
		StartCoroutine(routine);
		
		return completionSource.Task;
	}

	async void SetDisc(int _Accuracy)
	{
		int accuracy = Mathf.Min(m_TargetAccuracy, _Accuracy);
		
		ScoreRank rank = GetRank(accuracy);
		
		if (m_CoinsRank < rank)
		{
			m_CoinsRank   =  rank;
			m_Coins.Value += m_SongsManager.GetPayout(m_SongID, ScoreRank.None);
		}
		
		if (m_DiscRank >= rank)
			return;
		
		m_DiscRank = rank;
		
		m_Coins.Value += m_SongsManager.GetPayout(m_SongID, m_DiscRank);
		
		string sound = GetDiscSound(rank);
		
		if (!string.IsNullOrEmpty(sound))
			m_SoundProcessor.Play(sound);
		
		m_DiscToken?.Cancel();
		m_DiscToken?.Dispose();
		
		m_DiscToken = new CancellationTokenSource();
		
		CancellationToken token = m_DiscToken.Token;
		
		Task ScaleDisc(Vector3 _Scale, float _Duration, EaseFunction _Function, CancellationToken _Token)
		{
			Vector3 source = m_Disc.RectTransform.localScale;
			Vector3 target = _Scale;
			
			return UnityTask.Phase(
				_Phase => m_Disc.RectTransform.localScale = Vector3.LerpUnclamped(source, target, _Phase),
				_Duration,
				_Function,
				_Token
			);
		}
		
		Vector3 scale = new Vector3(0, 1.5f, 1);
		
		m_Flare.Play();
		
		try
		{
			await ScaleDisc(scale, 0.15f, EaseFunction.EaseIn, token);
			
			m_Disc.RectTransform.localScale = scale;
			
			m_Disc.Rank = rank;
			
			await ScaleDisc(Vector3.one, 0.2f, EaseFunction.EaseOutBack, token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		m_DiscToken?.Dispose();
		m_DiscToken = null;
	}

	void SetAccuracy(int _Accuracy)
	{
		int upperAccuracy = _Accuracy + 1;
		foreach (UISongScoreElement upper in m_UpperElements)
		{
			upper.Setup(
				upperAccuracy,
				m_ScoreController.GetScore(upperAccuracy),
				GetDisc(upperAccuracy - 1),
				GetRank(upperAccuracy - 1),
				upperAccuracy == m_SourceAccuracy
			);
			upperAccuracy++;
		}
		
		int lowerAccuracy = _Accuracy;
		foreach (UISongScoreElement lower in m_LowerElements)
		{
			lower.Setup(
				lowerAccuracy,
				m_ScoreController.GetScore(lowerAccuracy),
				ScoreRank.None,
				GetRank(lowerAccuracy - 1),
				lowerAccuracy == m_SourceAccuracy
			);
			lowerAccuracy--;
		}
	}

	void SetMilestone(int _Accuracy)
	{
		int accuracy = _Accuracy + m_UpperElements.Length;
		
		if (accuracy < m_BronzeThreshold)
			m_Milestone.Accuracy = m_BronzeThreshold;
		else if (accuracy < m_SilverThreshold)
			m_Milestone.Accuracy = m_SilverThreshold;
		else if (accuracy < m_GoldThreshold)
			m_Milestone.Accuracy = m_GoldThreshold;
		else if (accuracy < m_PlatinumThreshold)
			m_Milestone.Accuracy = m_PlatinumThreshold;
		else
			m_Milestone.Hide();
	}

	IEnumerator ShiftRoutine(Action _Finished)
	{
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			float source = m_TargetAccuracy * m_Curve.Evaluate(time / m_Duration);
			
			time += Time.deltaTime;
			
			float target = m_TargetAccuracy * m_Curve.Evaluate(time / m_Duration);
			
			int sourceAccuracy = (int)source;
			int targetAccuracy = (int)target;
			
			if (targetAccuracy > sourceAccuracy)
			{
				m_SoundProcessor.Play(m_MoveSound);
				m_HapticProcessor.Process(Haptic.Type.ImpactMedium);
				
				SetDisc(targetAccuracy);
				
				SetAccuracy(targetAccuracy);
				
				SetMilestone(targetAccuracy);
			}
			
			float phase = target - targetAccuracy;
			
			ProcessPhase(phase);
		}
		
		SetAccuracy(m_TargetAccuracy);
		
		SetMilestone(m_TargetAccuracy);
		
		ProcessPhase(0);
		
		_Finished?.Invoke();
	}

	void ProcessPhase(float _Phase)
	{
		foreach (UISongScoreElement upper in m_UpperElements)
			upper.Phase = _Phase;
		foreach (UISongScoreElement lower in m_LowerElements)
			lower.Phase = _Phase;
	}
}
