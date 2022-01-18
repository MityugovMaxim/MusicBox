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
	[SerializeField] AnimationCurve m_Curve;

	CanvasGroup m_CanvasGroup;

	IEnumerator m_PlayRoutine;

	public void Play()
	{
		if (m_PlayRoutine != null)
			StopCoroutine(m_PlayRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_PlayRoutine = PlayRoutine(CanvasGroup, m_Duration, m_Curve);
		
		StartCoroutine(m_PlayRoutine);
	}

	static IEnumerator PlayRoutine(CanvasGroup _CanvasGroup, float _Duration, AnimationCurve _Curve)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = _Curve.Evaluate(time / _Duration);
			
			_CanvasGroup.alpha = Mathf.Lerp(1, 0, phase);
		}
		
		_CanvasGroup.alpha = 0;
	}
}