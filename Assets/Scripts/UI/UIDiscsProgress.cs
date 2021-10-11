using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIDiscsProgress : UIEntity
{
	static readonly int m_RestoreParameterID  = Animator.StringToHash("Restore");
	static readonly int m_ProgressParameterID = Animator.StringToHash("Progress");
	static readonly int m_ClaimParameterID    = Animator.StringToHash("Claim");
	static readonly int m_InstantParameterID  = Animator.StringToHash("Instant");

	[SerializeField]              UISplineProgress m_Progress;
	[SerializeField]              UIDisc           m_Disc;
	[SerializeField]              UIResultRank[]   m_Ranks;
	[SerializeField, Range(0, 1)] float            m_Phase;
	[SerializeField]              float            m_Source;
	[SerializeField]              float            m_Target;
	[SerializeField, Range(0, 1)] float            m_MinProgress;
	[SerializeField, Range(0, 1)] float            m_MaxProgress;

	ScoreProcessor m_ScoreProcessor;

	Animator m_Animator;

	StateBehaviour m_ProgressState;
	StateBehaviour m_ClaimState;

	Action m_ProgressFinished;
	Action m_ClaimFinished;

	ScoreRank m_Rank;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_ProgressState = StateBehaviour.GetBehaviour(m_Animator, "progress");
		if (m_ProgressState != null)
			m_ProgressState.OnComplete += InvokeProgressFinished;
		
		m_ClaimState = StateBehaviour.GetBehaviour(m_Animator, "claim");
		if (m_ClaimState != null)
			m_ClaimState.OnComplete += InvokeClaimFinished;
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessPhase();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessPhase();
	}
	#endif

	[Inject]
	public void Construct(ScoreProcessor _ScoreProcessor)
	{
		m_ScoreProcessor = _ScoreProcessor;
	}

	public void Setup()
	{
		CancellationTokenSource tokenSource = m_TokenSource;
		m_TokenSource = null;
		if (tokenSource != null)
		{
			tokenSource.Cancel();
			tokenSource.Dispose();
		}
		
		int sourceAccuracy = m_ScoreProcessor.OriginAccuracy;
		int targetAccuracy = Mathf.Max(m_ScoreProcessor.OriginAccuracy, m_ScoreProcessor.Accuracy);
		
		m_Phase = 0;
		m_Rank  = m_ScoreProcessor.OriginRank;
		int minAccuracy = m_ScoreProcessor.GetRankMinAccuracy(m_Rank);
		int maxAccuracy = m_ScoreProcessor.GetRankMaxAccuracy(m_Rank);
		m_Source = Mathf.InverseLerp(minAccuracy, maxAccuracy, sourceAccuracy);
		m_Target = Mathf.InverseLerp(minAccuracy, maxAccuracy, targetAccuracy);
		
		foreach (UIResultRank rankFX in m_Ranks)
		{
			if (rankFX != null)
				rankFX.Setup(m_Rank);
		}
		
		ProcessRank();
		
		ProcessPhase();
		
		Restore();
	}

	public async Task Play()
	{
		CancellationTokenSource tokenSource = m_TokenSource;
		m_TokenSource = null;
		if (tokenSource != null)
		{
			tokenSource.Cancel();
			tokenSource.Dispose();
		}
		
		m_TokenSource = new CancellationTokenSource();
		
		ScoreRank sourceRank     = m_ScoreProcessor.OriginRank;
		ScoreRank targetRank     = m_ScoreProcessor.OriginRank.Max(m_ScoreProcessor.Rank);
		int       sourceAccuracy = m_ScoreProcessor.OriginAccuracy;
		int       targetAccuracy = Mathf.Max(m_ScoreProcessor.OriginAccuracy, m_ScoreProcessor.Accuracy);
		
		ScoreRank rank = sourceRank;
		do
		{
			await Play(rank, rank.Next().Min(targetRank), sourceAccuracy, targetAccuracy, m_TokenSource.Token);
			
			rank = rank.Next().Min(targetRank);
		}
		while (rank < targetRank);
		
		if (m_TokenSource != null)
		{
			m_TokenSource.Dispose();
			m_TokenSource = null;
		}
	}

	[ContextMenu("Test")]
	public async Task Test()
	{
		CancellationTokenSource tokenSource = m_TokenSource;
		m_TokenSource = null;
		if (tokenSource != null)
		{
			tokenSource.Cancel();
			tokenSource.Dispose();
		}
		
		// Setup
		
		Restore();
		
		ScoreRank sourceRank     = ScoreRank.Platinum;
		ScoreRank targetRank     = ScoreRank.Platinum;
		int       sourceAccuracy = 97;
		int       targetAccuracy = 99;
		
		m_Phase = 0;
		m_Rank  = sourceRank;
		int minAccuracy = m_ScoreProcessor.GetRankMinAccuracy(m_Rank);
		int maxAccuracy = m_ScoreProcessor.GetRankMaxAccuracy(m_Rank);
		m_Source = Mathf.InverseLerp(minAccuracy, maxAccuracy, sourceAccuracy);
		m_Target = Mathf.InverseLerp(minAccuracy, maxAccuracy, targetAccuracy);
		
		foreach (UIResultRank rankFX in m_Ranks)
		{
			if (rankFX != null)
				rankFX.Setup(m_Rank);
		}
		
		ProcessRank();
		
		ProcessPhase();
		
		// Play
		m_TokenSource = new CancellationTokenSource();
		
		ScoreRank rank = sourceRank;
		do
		{
			await Play(rank, rank.Next().Min(targetRank), sourceAccuracy, targetAccuracy, m_TokenSource.Token);
			
			rank = rank.Next().Min(targetRank);
		}
		while (rank < targetRank);
		
		await Progress(targetRank, sourceAccuracy, targetAccuracy, m_TokenSource.Token);
		
		if (m_TokenSource != null)
		{
			m_TokenSource.Dispose();
			m_TokenSource = null;
		}
	}

	async Task Play(
		ScoreRank         _SourceRank,
		ScoreRank         _TargetRank,
		int               _SourceAccuracy,
		int               _TargetAccuracy,
		CancellationToken _Token = default
	)
	{
		if (_Token.IsCancellationRequested)
			return;
		
		m_Animator.SetBool(m_InstantParameterID, _SourceRank == ScoreRank.Platinum);
		
		if (_SourceAccuracy < _TargetAccuracy)
			await Progress(_SourceRank, _SourceAccuracy, _TargetAccuracy, _Token);
		
		if (_SourceRank < _TargetRank)
			await Claim(_TargetRank, _Token);
		
		m_Animator.SetBool(m_InstantParameterID, false);
	}

	Task Progress(ScoreRank _Rank, int _SourceAccuracy, int _TargetAccuracy, CancellationToken _Token = default)
	{
		InvokeProgressFinished();
		
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		m_ProgressFinished = () => taskSource.SetResult(true);
		
		if (_Token.IsCancellationRequested)
		{
			InvokeProgressFinished();
			return taskSource.Task;
		}
		
		_Token.Register(InvokeProgressFinished);
		
		int minAccuracy = m_ScoreProcessor.GetRankMinAccuracy(_Rank);
		int maxAccuracy = m_ScoreProcessor.GetRankMaxAccuracy(_Rank);
		
		m_Source = Mathf.InverseLerp(minAccuracy, maxAccuracy, _SourceAccuracy);
		m_Target = Mathf.InverseLerp(minAccuracy, maxAccuracy, _TargetAccuracy);
		m_Animator.SetTrigger(m_ProgressParameterID);
		
		return taskSource.Task;
	}

	Task Claim(ScoreRank _Rank, CancellationToken _Token = default)
	{
		InvokeClaimFinished();
		
		TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
		
		m_ClaimFinished = () => taskSource.SetResult(true);
		
		if (_Token.IsCancellationRequested)
		{
			InvokeClaimFinished();
			return taskSource.Task;
		}
		
		_Token.Register(InvokeClaimFinished);
		
		m_Rank   = _Rank;
		m_Source = 0;
		m_Target = 1;
		
		m_Animator.SetTrigger(m_ClaimParameterID);
		
		return taskSource.Task;
	}

	void Restore()
	{
		InvokeProgressFinished();
		InvokeClaimFinished();
		
		m_Animator.SetBool(m_InstantParameterID, false);
		m_Animator.ResetTrigger(m_ProgressParameterID);
		m_Animator.ResetTrigger(m_ClaimParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
	}

	[Preserve]
	void SwitchRank()
	{
		ProcessRank();
		
		foreach (UIResultRank rankFX in m_Ranks)
		{
			if (rankFX != null)
				rankFX.Unlock(m_Rank);
		}
	}

	void ProcessRank()
	{
		m_Disc.Rank = m_Rank;
	}

	void ProcessPhase()
	{
		m_Progress.Min = m_MinProgress;
		m_Progress.Max = Mathf.Lerp(m_MinProgress, m_MaxProgress, Mathf.Lerp(m_Source, m_Target, m_Phase));
	}

	void InvokeProgressFinished()
	{
		Action action = m_ProgressFinished;
		m_ProgressFinished = null;
		action?.Invoke();
	}

	void InvokeClaimFinished()
	{
		Action action = m_ClaimFinished;
		m_ClaimFinished = null;
		action?.Invoke();
	}
}