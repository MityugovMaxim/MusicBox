using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
public class UIMultiplierProgress : UIOrder
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");

	public float Phase
	{
		get => m_Phase;
		set
		{
			if (Mathf.Approximately(m_Phase, value))
				return;
			
			m_Phase = value;
			
			ProcessPhase();
		}
	}

	[SerializeField, Range(0, 1)] float m_Phase;

	[SerializeField] UILine         m_Progress;
	[SerializeField] float          m_MinProgress = 0;
	[SerializeField] float          m_MaxProgress = 1;
	[SerializeField] float          m_Duration    = 0.2f;
	[SerializeField] AnimationCurve m_Curve       = AnimationCurve.Linear(0, 0, 1, 1);

	Animator    m_Animator;
	IEnumerator m_ProgressRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessPhase();
	}
	#endif

	public void Restore()
	{
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	public void Progress(float _Progress, bool _Instant = false)
	{
		if (m_ProgressRoutine != null)
			StopCoroutine(m_ProgressRoutine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_Phase = 0;
			
			ProcessPhase();
			
			return;
		}
		
		m_ProgressRoutine = ProgressRoutine(_Progress);
		
		StartCoroutine(m_ProgressRoutine);
	}

	public void Play(float _Progress)
	{
		Restore();
		
		m_Phase = _Progress;
		
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	IEnumerator ProgressRoutine(float _Progress)
	{
		float source = Phase;
		float target = _Progress;
		float time   = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			Phase = Mathf.Lerp(source, target, m_Curve.Evaluate(time / m_Duration));
		}
		
		Phase = target;
	}

	void ProcessPhase()
	{
		m_Progress.Max = Mathf.Lerp(m_MinProgress, m_MaxProgress, Phase);
	}
}