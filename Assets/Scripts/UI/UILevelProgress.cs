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

	[SerializeField] RectTransform     m_Target;
	[SerializeField] UICascadeTMPLabel m_Label;
	[SerializeField] UILevel           m_SourceLevel;
	[SerializeField] UILevel           m_TargetLevel;
	[SerializeField] float             m_ProgressDelay;
	[SerializeField] float             m_ProgressDuration;
	[SerializeField] AnimationCurve    m_ProgressCurve;

	[SerializeField, Range(0, 1)] float m_Progress;

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

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessProgress();
	}
	#endif

	public void Setup(
		int   _SourceLevel,
		int   _TargetLevel,
		float _SourceProgress,
		float _TargetProgress
	)
	{
		m_SourceLevel.Level = _SourceLevel;
		m_TargetLevel.Level = _TargetLevel;
		
		m_Progress       = _SourceProgress;
		m_SourceProgress = _SourceProgress;
		m_TargetProgress = _TargetProgress;
		
		m_Target.anchorMin = new Vector2(0, 0);
		m_Target.anchorMax = new Vector2(m_SourceProgress, 1);
		
		m_Label.Text = m_LocalizationProcessor.Get("RESULT_LEVEL_UP");
		
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
		
		m_CollectFinished = () => completionSource.SetResult(true);
		
		if (m_Animator != null)
			m_Animator.SetTrigger(m_CollectParameterID);
		else
			InvokeCollectFinished();
		
		return completionSource.Task;
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

	[Preserve]
	void LevelUp()
	{
		m_SoundProcessor.Play(m_LevelSound);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		m_Label.Play();
	}

	void ProcessProgress()
	{
		m_Target.anchorMax = new Vector2(Progress, 1);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Restore();
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}
}