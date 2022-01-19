using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UILevelProgress : UIGroup
{
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] UISplineProgress  m_Progress;
	[SerializeField] UICascadeTMPLabel m_Label;
	[SerializeField] UILevel           m_SourceLevel;
	[SerializeField] UILevel           m_TargetLevel;
	[SerializeField] float             m_ProgressDelay;
	[SerializeField] float             m_ProgressDuration;
	[SerializeField] AnimationCurve    m_ProgressCurve;

	LanguageProcessor m_LanguageProcessor;
	HapticProcessor   m_HapticProcessor;

	float m_SourceProgress;
	float m_TargetProgress;

	Animator m_Animator;
	Action   m_CollectFinished;
	Action   m_ProgressFinished;

	IEnumerator m_ProgressRoutine;

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

	[Inject]
	public void Construct(
		LanguageProcessor _LanguageProcessor,
		HapticProcessor   _HapticProcessor
	)
	{
		m_LanguageProcessor = _LanguageProcessor;
		m_HapticProcessor   = _HapticProcessor;
	}

	public void Setup(
		int   _SourceLevel,
		int   _TargetLevel,
		float _SourceProgress,
		float _TargetProgress
	)
	{
		m_SourceLevel.Level = _SourceLevel;
		m_TargetLevel.Level = _TargetLevel;
		
		m_SourceProgress = _SourceProgress;
		m_TargetProgress = _TargetProgress;
		
		m_Progress.Min = 0;
		m_Progress.Max = m_SourceProgress;
		
		m_Label.Text = m_LanguageProcessor.Get("RESULT_LEVEL_UP");
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

	public Task Collect()
	{
		InvokeCollectFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_CollectFinished = () => completionSource.SetResult(true);
		
		if (m_Animator != null)
			m_Animator.SetTrigger(m_CollectParameterID);
		else
			InvokeCollectFinished();
		
		return completionSource.Task;
	}

	[Preserve]
	void LevelUp()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		m_Label.Play();
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Restore();
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
				
				m_Progress.Min = 0;
				m_Progress.Max = Mathf.Lerp(m_SourceProgress, m_TargetProgress, phase);
			}
		}
		
		m_Progress.Min = 0;
		m_Progress.Max = m_TargetProgress;
		
		InvokeProgressFinished();
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}

	void InvokeProgressFinished()
	{
		Action action = m_ProgressFinished;
		m_ProgressFinished = null;
		action?.Invoke();
	}
}