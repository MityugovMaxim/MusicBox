using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
public class UIMultiplierProgress : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");

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

	[SerializeField] UIRing         m_Ring;
	[SerializeField] float          m_Progress;
	[SerializeField] float          m_Duration = 0.2f;
	[SerializeField] AnimationCurve m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);

	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.RegisterComplete("play", InvokePlayFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnregisterComplete("play", InvokePlayFinished);
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessProgress();
	}
	#endif

	public void Restore()
	{
		InvokePlayFinished();
		
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	public Task ProgressAsync(float _Progress, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.FromCanceled(_Token);
		
		return UnityTask.Lerp(
			_Value => Progress = _Value,
			Progress,
			_Progress,
			m_Duration,
			m_Curve,
			_Token
		);
	}

	public Task PlayAsync()
	{
		Restore();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_PlayFinished = () => completionSource.TrySetResult(true);
		
		m_Animator.SetTrigger(m_PlayParameterID);
		
		return completionSource.Task;
	}

	void ProcessProgress()
	{
		m_Ring.Arc = Progress;
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}