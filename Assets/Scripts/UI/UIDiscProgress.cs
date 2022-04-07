using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIDiscProgress : UIGroup
{
	public ScoreRank Rank => m_Rank;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");

	[SerializeField] ScoreRank        m_Rank;
	[SerializeField] UISplineProgress m_Progress;
	[SerializeField] float            m_ProgressDelay;
	[SerializeField] float            m_ProgressDuration;
	[SerializeField] AnimationCurve   m_ProgressCurve;
	[SerializeField] float            m_MinProgress;
	[SerializeField] float            m_MaxProgress;

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
		
		StateBehaviour.RegisterComplete(m_Animator, "collect", InvokeCollectFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		StateBehaviour.UnregisterComplete(m_Animator, "collect", InvokeCollectFinished);
	}

	public void Setup(float _SourceProgress, float _TargetProgress)
	{
		m_SourceProgress = _SourceProgress;
		m_TargetProgress = _TargetProgress;
		
		m_Progress.Min = GetProgress(0);
		m_Progress.Max = GetProgress(m_SourceProgress);
	}

	public Task Progress()
	{
		m_SoundProcessor.Start(m_ProgressSound);
		
		m_Progress.Min = GetProgress(0);
		m_Progress.Max = GetProgress(m_SourceProgress);
		
		return UnityTask.Phase(
			_Phase =>
			{
				float progress = Mathf.Lerp(m_SourceProgress, m_TargetProgress, m_ProgressCurve.Evaluate(_Phase));
				m_Progress.Max = GetProgress(progress);
			},
			m_ProgressDelay,
			m_ProgressDuration
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
		m_Animator.Update(0);
	}

	float GetProgress(float _Progress)
	{
		return Mathf.Lerp(m_MinProgress, m_MaxProgress, _Progress);
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}
}