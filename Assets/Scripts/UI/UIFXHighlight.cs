using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFXHighlight : UIEntity
{
	CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroup == null)
				m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	[SerializeField] float          m_Duration;
	[SerializeField] float          m_Source = 1;
	[SerializeField] float          m_Target = 0;
	[SerializeField] AnimationCurve m_Curve  = AnimationCurve.Linear(0, 0, 1, 1);

	CanvasGroup m_CanvasGroup;

	IEnumerator m_PlayRoutine;

	public void Play()
	{
		if (m_PlayRoutine != null)
			StopCoroutine(m_PlayRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_PlayRoutine = PlayRoutine(CanvasGroup, m_Source, m_Target, m_Duration, m_Curve);
		
		StartCoroutine(m_PlayRoutine);
	}

	static IEnumerator PlayRoutine(CanvasGroup _CanvasGroup, float _Source, float _Target, float _Duration, AnimationCurve _Curve)
	{
		if (_CanvasGroup == null)
			yield break;
		
		_CanvasGroup.alpha = _Source;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = _Curve.Evaluate(time / _Duration);
			
			_CanvasGroup.alpha = Mathf.Lerp(_Source, _Target, phase);
		}
		
		_CanvasGroup.alpha = 0;
	}
}