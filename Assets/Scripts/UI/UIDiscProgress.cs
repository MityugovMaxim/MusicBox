using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIDiscProgress : UIGroup
{
	public ScoreRank Rank => m_Rank;

	public float Progress
	{
		get => m_Progress;
		set
		{
			if (Mathf.Approximately(m_Progress, value))
				return;
			
			m_Progress = value;
			
			ProcessProgress();
		}
	}

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");

	[SerializeField] ScoreRank        m_Rank;
	[SerializeField] UISplineProgress m_Target;
	[SerializeField] float            m_ProgressDelay;
	[SerializeField] float            m_ProgressDuration;
	[SerializeField] AnimationCurve   m_ProgressCurve;
	[SerializeField] float            m_MinProgress;
	[SerializeField] float            m_MaxProgress;

	[SerializeField, Range(0, 1)] float m_Progress;

	[Header("Sounds")]
	[SerializeField, Sound] string m_ProgressSound;
	[SerializeField, Sound] string m_CollectSound;

	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	float m_SourceProgress;
	float m_TargetProgress;

	Animator m_Animator;
	Action   m_CollectFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_Animator.RegisterComplete("collect", InvokeCollectFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnregisterComplete("collect", InvokeCollectFinished);
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessProgress();
	}
	#endif

	public void Setup(float _SourceProgress, float _TargetProgress)
	{
		m_Progress       = _SourceProgress;
		m_SourceProgress = _SourceProgress;
		m_TargetProgress = _TargetProgress;
		
		ProcessProgress();
	}

	public Task ProgressAsync()
	{
		m_SoundProcessor.Start(m_ProgressSound);
		
		return UnityTask.Lerp(
			_Value => Progress = _Value,
			m_SourceProgress,
			m_TargetProgress,
			m_ProgressDelay,
			m_ProgressDuration,
			m_ProgressCurve
		).ContinueWithOnMainThread(_Task => m_SoundProcessor.Stop(m_ProgressSound));
	}

	public Task CollectAsync()
	{
		InvokeCollectFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_CollectFinished = () => completionSource.TrySetResult(true);
		
		m_SoundProcessor.Play(m_CollectSound);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		if (m_Animator != null)
			m_Animator.SetTrigger(m_CollectParameterID);
		else
			InvokeCollectFinished();
		
		return completionSource.Task;
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Restore();
	}

	void Restore()
	{
		InvokeCollectFinished();
		
		if (m_Animator == null)
			return;
		
		m_Animator.ResetTrigger(m_CollectParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	void ProcessProgress()
	{
		m_Target.Min = m_MinProgress;
		m_Target.Max = Mathf.Lerp(m_MinProgress, m_MaxProgress, Progress);
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}
}