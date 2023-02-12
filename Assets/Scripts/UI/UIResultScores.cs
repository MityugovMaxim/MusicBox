using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultScores : UIGroup
{
	public event Action<int> OnAccuracyChange;

	float Position
	{
		get => m_Position;
		set
		{
			if (Mathf.Approximately(m_Position, value))
				return;
			
			m_Position = value;
			
			ProcessPosition();
		}
	}

	[SerializeField] UISongImage          m_Image;
	[SerializeField] UISongLabel          m_Label;
	[SerializeField] UIUnitLabel          m_Score;
	[SerializeField] UIDisc               m_Disc;
	[SerializeField] UIFlare              m_Flare;
	[SerializeField] UISongScoreMilestone m_Milestone;
	[SerializeField] UISongScoreElement[] m_UpperElements;
	[SerializeField] UISongScoreElement[] m_LowerElements;
	[SerializeField] AnimationCurve       m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] float                m_Duration = 3;
	[SerializeField] float                m_Position;
	[SerializeField] int                  m_Accuracy;

	[SerializeField, Sound] string m_MoveSound;
	[SerializeField, Sound] string m_PlatinumSound;
	[SerializeField, Sound] string m_GoldSound;
	[SerializeField, Sound] string m_SilverSound;
	[SerializeField, Sound] string m_BronzeSound;

	[Inject] ScoresManager     m_ScoresManager;
	[Inject] ScoreController   m_ScoreController;
	[Inject] SongsManager      m_SongsManager;
	[Inject] DifficultyManager m_DifficultyManager;
	[Inject] SoundProcessor    m_SoundProcessor;
	[Inject] HapticProcessor   m_HapticProcessor;

	CancellationTokenSource m_DiscToken;
	RankType                m_DiscRank;

	string m_SongID;

	int m_SourceAccuracy;
	int m_TargetAccuracy;

	int m_BronzeThreshold;
	int m_SilverThreshold;
	int m_GoldThreshold;
	int m_PlatinumThreshold;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying || !IsInstanced)
			return;
		
		ProcessPosition();
	}
	#endif

	public void Setup(string _SongID)
	{
		m_SongID         = _SongID;
		m_SourceAccuracy = m_ScoresManager.GetAccuracy(m_SongID);
		m_TargetAccuracy = m_ScoreController.Accuracy;
		
		RankType songRank = m_SongsManager.GetRank(m_SongID);
		
		m_BronzeThreshold   = m_DifficultyManager.GetThreshold(songRank, RankType.Bronze);
		m_SilverThreshold   = m_DifficultyManager.GetThreshold(songRank, RankType.Silver);
		m_GoldThreshold     = m_DifficultyManager.GetThreshold(songRank, RankType.Gold);
		m_PlatinumThreshold = m_DifficultyManager.GetThreshold(songRank, RankType.Platinum);
		
		m_DiscRank = m_ScoresManager.GetRank(m_SongID);
		
		Position = m_SourceAccuracy;
		
		m_Image.SongID = m_SongID;
		m_Label.SongID = m_SongID;
		
		m_Milestone.Setup(m_SongID);
	}

	protected override void OnShowStarted()
	{
		m_Disc.Rank   = m_DiscRank;
		m_Score.Value = 0;
	}

	public async Task PlayAsync()
	{
		await Task.Delay(100);
		
		await Task.WhenAll(
			ScoreAsync(m_ScoreController.Score),
			PositionAsync()
		);
		
		await Task.Delay(250);
	}

	RankType GetDisc(int _Accuracy)
	{
		if (_Accuracy == m_PlatinumThreshold)
			return RankType.Platinum;
		if (_Accuracy == m_GoldThreshold)
			return RankType.Gold;
		if (_Accuracy == m_SilverThreshold)
			return RankType.Silver;
		if (_Accuracy == m_BronzeThreshold)
			return RankType.Bronze;
		return RankType.None;
	}

	RankType GetRank(int _Accuracy)
	{
		if (_Accuracy >= m_PlatinumThreshold)
			return RankType.Platinum;
		if (_Accuracy >= m_GoldThreshold)
			return RankType.Gold;
		if (_Accuracy >= m_SilverThreshold)
			return RankType.Silver;
		if (_Accuracy >= m_BronzeThreshold)
			return RankType.Bronze;
		return RankType.None;
	}

	string GetDiscSound(RankType _Rank)
	{
		switch (_Rank)
		{
			case RankType.Platinum: return m_PlatinumSound;
			case RankType.Gold:     return m_GoldSound;
			case RankType.Silver:   return m_SilverSound;
			case RankType.Bronze:   return m_BronzeSound;
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

	Task PositionAsync()
	{
		TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
		
		IEnumerator routine = PositionRoutine(() => task.TrySetResult(true));
		
		StartCoroutine(routine);
		
		return task.Task;
	}

	async void SetDisc(int _Accuracy)
	{
		int accuracy = Mathf.Min(m_TargetAccuracy, _Accuracy);
		
		RankType rank = GetRank(accuracy);
		
		if (m_DiscRank >= rank)
			return;
		
		m_DiscRank = rank;
		
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
				m_ScoreController?.GetScore(upperAccuracy) ?? 0,
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
				m_ScoreController?.GetScore(lowerAccuracy) ?? 0,
				RankType.None,
				GetRank(lowerAccuracy - 1),
				lowerAccuracy == m_SourceAccuracy
			);
			lowerAccuracy--;
		}
	}

	void SetMilestone(int _Accuracy)
	{
		int accuracy = _Accuracy + m_UpperElements.Length - 2;
		
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

	IEnumerator PositionRoutine(Action _Finished)
	{
		Position = m_SourceAccuracy;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			Position = Mathf.Lerp(m_SourceAccuracy, m_TargetAccuracy, phase);
		}
		
		Position = m_TargetAccuracy;
		
		_Finished?.Invoke();
	}

	void ProcessPosition()
	{
		int accuracy = Mathf.FloorToInt(Position);
		
		float phase = Mathf.Repeat(Position, 1);
		
		ProcessPhase(phase);
		
		if (m_Accuracy == accuracy)
			return;
		
		m_Accuracy = accuracy;
		
		m_SoundProcessor.Play(m_MoveSound);
		m_HapticProcessor.Process(Haptic.Type.ImpactMedium);
		
		SetAccuracy(m_Accuracy);
		
		SetMilestone(m_Accuracy);
		
		SetDisc(m_Accuracy);
		
		OnAccuracyChange?.Invoke(m_Accuracy);
	}

	void ProcessPhase(float _Phase)
	{
		foreach (UISongScoreElement upper in m_UpperElements)
			upper.Phase = _Phase;
		foreach (UISongScoreElement lower in m_LowerElements)
			lower.Phase = _Phase;
	}
}
