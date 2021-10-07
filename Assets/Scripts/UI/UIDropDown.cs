using System.Collections;
using UnityEngine;

public class UIDropDown : UIEntity
{
	[SerializeField, Range(0, 1)] float          m_Phase;
	[SerializeField]              float          m_Delay;
	[SerializeField]              float          m_Duration;
	[SerializeField]              AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField]              RectTransform  m_Content;
	[SerializeField]              CanvasGroup    m_CanvasGroup;
	[SerializeField]              float          m_SourceAlpha;
	[SerializeField]              float          m_TargetAlpha;
	[SerializeField]              Vector2        m_SourcePosition;
	[SerializeField]              Vector2        m_TargetPosition;

	bool m_Shown;

	IEnumerator m_PhaseRoutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessPhase();
	}

	void Update()
	{
		if (m_Shown && Input.GetMouseButtonDown(0))
		{
			bool focus = RectTransformUtility.RectangleContainsScreenPoint(
				RectTransform,
				Input.mousePosition,
				null
			);
			
			if (!focus)
				Hide(false);
		}
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessPhase();
	}
	#endif

	public void Toggle(bool _Instant)
	{
		if (m_Shown)
			Hide(_Instant);
		else
			Show(_Instant);
	}

	public void Show(bool _Instant)
	{
		if (m_Shown)
			return;
		
		m_Shown = true;
		
		if (m_PhaseRoutine != null)
			StopCoroutine(m_PhaseRoutine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_Phase = 1;
			
			ProcessPhase();
		}
		else
		{
			m_PhaseRoutine = PhaseRoutine(1);
			
			StartCoroutine(m_PhaseRoutine);
		}
	}

	public void Hide(bool _Instant)
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		if (m_PhaseRoutine != null)
			StopCoroutine(m_PhaseRoutine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_Phase = 0;
			
			ProcessPhase();
		}
		else
		{
			m_PhaseRoutine = PhaseRoutine(0);
			
			StartCoroutine(m_PhaseRoutine);
		}
	}

	IEnumerator PhaseRoutine(float _Phase)
	{
		if (m_Delay > float.Epsilon)
			yield return new WaitForSeconds(m_Delay);
		
		float source = m_Phase;
		float target = Mathf.Clamp01(_Phase);
		
		if (!Mathf.Approximately(source, target))
		{
			float time     = 0;
			float duration = m_Duration * Mathf.Abs(target - source);
			while (time < duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				m_Phase = Mathf.Lerp(source, target, m_Curve.Evaluate(time / duration));
				
				ProcessPhase();
			}
		}
		
		m_Phase = target;
		
		ProcessPhase();
	}

	void ProcessPhase()
	{
		m_Content.anchoredPosition = Vector2.Lerp(
			m_SourcePosition,
			m_TargetPosition,
			m_Phase
		);
		
		m_CanvasGroup.alpha = Mathf.Lerp(
			m_SourceAlpha,
			m_TargetAlpha,
			m_Phase
		);
	}
}
