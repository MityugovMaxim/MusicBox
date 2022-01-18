using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIDiscProgress : UIGroup
{
	public ScoreRank Rank => m_Rank;

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");

	[SerializeField] ScoreRank m_Rank;

	[Space(15)]
	[SerializeField] UISplineProgress m_Progress;
	[SerializeField] float            m_ProgressDelay;
	[SerializeField] float            m_ProgressDuration;
	[SerializeField] AnimationCurve   m_ProgressCurve;
	[SerializeField] float            m_MinProgress;
	[SerializeField] float            m_MaxProgress;

	HapticProcessor m_HapticProcessor;

	float m_SourceProgress;
	float m_TargetProgress;

	Animator    m_Animator;
	IEnumerator m_ProgressRoutine;
	Action      m_CollectFinished;
	Action      m_ProgressFinished;

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

	[Inject]
	public void Construct(HapticProcessor _HapticProcessor)
	{
		m_HapticProcessor = _HapticProcessor;
	}

	public Task Progress()
	{
		if (m_ProgressRoutine != null)
			StopCoroutine(m_ProgressRoutine);
		
		InvokeProgressFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_ProgressFinished = () => completionSource.SetResult(true);
		
		m_ProgressRoutine = ProgressRoutine();
		
		StartCoroutine(m_ProgressRoutine);
		
		return completionSource.Task;
	}

	public void Collect()
	{
		InvokeCollectFinished();
		
		m_CollectFinished = () => Hide();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactRigid);
		
		if (m_Animator != null)
			m_Animator.SetTrigger(m_CollectParameterID);
		else
			InvokeCollectFinished();
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Restore();
	}

	IEnumerator ProgressRoutine()
	{
		if (m_ProgressDelay > float.Epsilon)
			yield return new WaitForSeconds(m_ProgressDelay);
		
		if (!Mathf.Approximately(m_SourceProgress, m_TargetProgress) && m_ProgressDuration > float.Epsilon)
		{
			m_HapticProcessor.Play(Haptic.Type.ImpactLight, 8, m_ProgressDuration);
			
			float time = 0;
			while (time < m_ProgressDuration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = m_ProgressCurve.Evaluate(time / m_ProgressDuration);
				
				m_Progress.Min = GetProgress(0);
				m_Progress.Max = GetProgress(Mathf.Lerp(m_SourceProgress, m_TargetProgress, phase));
			}
		}
		
		m_Progress.Min = GetProgress(0);
		m_Progress.Max = GetProgress(m_TargetProgress);
		
		InvokeProgressFinished();
	}

	void Restore()
	{
		if (m_ProgressRoutine != null)
			StopCoroutine(m_ProgressRoutine);
		
		InvokeProgressFinished();
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

	void InvokeProgressFinished()
	{
		Action action = m_ProgressFinished;
		m_ProgressFinished = null;
		action?.Invoke();
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}
}