using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
public class UIMultiplierProgress : UIEntity
{
	Animator Animator
	{
		get
		{
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
				m_Animator.keepAnimatorControllerStateOnDisable = true;
			}
			return m_Animator;
		}
	}

	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	[SerializeField] UICircle       m_Progress;
	[SerializeField] float          m_Duration = 0.2f;
	[SerializeField] AnimationCurve m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);

	Animator    m_Animator;
	Action      m_ProgressFinished;
	Action      m_PlayFinished;
	IEnumerator m_ProgressRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		StateBehaviour.RegisterComplete(Animator, "play", InvokePlayFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		StateBehaviour.UnregisterComplete(Animator, "play", InvokePlayFinished);
	}

	public Task ProgressAsync(float _Progress, bool _Instant = false)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		Progress(_Progress, _Instant, () => completionSource.SetResult(true));
		
		return completionSource.Task;
	}

	public Task PlayAsync(bool _Instant = false)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		Play(_Instant, () => completionSource.SetResult(true));
		
		return completionSource.Task;
	}

	public void Progress(float _Progress, bool _Instant = false, Action _Finished = null)
	{
		if (m_ProgressRoutine != null)
			StopCoroutine(m_ProgressRoutine);
		
		InvokeProgressFinished();
		
		m_ProgressFinished = _Finished;
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_ProgressRoutine = ProgressRoutine(_Progress);
			
			StartCoroutine(m_ProgressRoutine);
		}
		else
		{
			m_Progress.Arc = Mathf.Clamp01(_Progress);
			
			InvokeProgressFinished();
		}
	}

	public void Play(bool _Instant = false, Action _Finished = null)
	{
		InvokePlayFinished();
		
		m_PlayFinished = _Finished;
		
		if (!_Instant && gameObject.activeInHierarchy)
			Animator.SetTrigger(m_PlayParameterID);
		else
			InvokePlayFinished();
	}

	IEnumerator ProgressRoutine(float _Progress)
	{
		float source = m_Progress.Arc;
		float target = Mathf.Clamp01(_Progress);
		if (!Mathf.Approximately(source, target))
		{
			float time   = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = m_Curve.Evaluate(time / m_Duration);
				
				m_Progress.Arc = Mathf.Lerp(source, target, phase);
			}
		}
		m_Progress.Arc = target;
		
		InvokeProgressFinished();
	}

	void InvokeProgressFinished()
	{
		Action action = m_ProgressFinished;
		m_ProgressFinished = null;
		action?.Invoke();
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}