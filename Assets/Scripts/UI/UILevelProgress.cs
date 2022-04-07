using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UILevelProgress : UIGroup
{
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] RectTransform     m_Progress;
	[SerializeField] UICascadeTMPLabel m_Label;
	[SerializeField] UILevel           m_SourceLevel;
	[SerializeField] UILevel           m_TargetLevel;
	[SerializeField] float             m_ProgressDelay;
	[SerializeField] float             m_ProgressDuration;
	[SerializeField] AnimationCurve    m_ProgressCurve;

	[Header("Sounds")]
	[SerializeField, Sound] string m_ProgressSound;
	[SerializeField, Sound] string m_LevelSound;

	[Inject] LocalizationProcessor m_LocalizationProcessor;
	[Inject] SoundProcessor        m_SoundProcessor;
	[Inject] HapticProcessor       m_HapticProcessor;

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
		
		m_Progress.anchorMin = new Vector2(0, 0);
		m_Progress.anchorMax = new Vector2(m_SourceProgress, 1);
		
		m_Label.Text = m_LocalizationProcessor.Get("RESULT_LEVEL_UP");
	}

	public Task Progress()
	{
		Vector2 source = new Vector2(m_SourceProgress, 1);
		Vector2 target = new Vector2(m_TargetProgress, 1);
		
		m_SoundProcessor.Start(m_ProgressSound);
		
		return UnityTask.Phase(
			_Phase => m_Progress.anchorMax = Vector2.Lerp(source, target, m_ProgressCurve.Evaluate(_Phase)),
			m_ProgressDuration,
			m_ProgressDelay
		).ContinueWithOnMainThread(_Task => m_SoundProcessor.Stop(m_ProgressSound));
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
		m_SoundProcessor.Play(m_LevelSound);
		
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
		InvokeCollectFinished();
		
		if (m_Animator == null)
			return;
		
		m_Animator.ResetTrigger(m_CollectParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}
}