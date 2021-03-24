using System;
using System.Collections;
using UnityEngine;

public class CommonInputIndicatorView : InputIndicatorView
{
	[SerializeField]              Indicator      m_Indicator;
	[SerializeField]              AnimationCurve m_AlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);
	[SerializeField, Range(0, 1)] float          m_SourceRadius;
	[SerializeField, Range(0, 1)] float          m_TargetRadius;
	[SerializeField, Range(0, 1)] float          m_ZoneMin;
	[SerializeField, Range(0, 1)] float          m_ZoneMax;

	bool   m_Complete;
	Action m_CompleteCallback;

	public override void Begin()
	{
		m_Complete = false;
	}

	public override void Process(float _Time)
	{
		if (m_Complete)
			return;
		
		CanvasGroup.alpha = m_AlphaCurve.Evaluate(_Time);
		
		float offset = (m_ZoneMax - m_ZoneMin) * 0.5f;
		
		float minRadius = Mathf.Lerp(m_SourceRadius, m_TargetRadius, _Time - offset);
		float maxRadius = Mathf.Lerp(m_SourceRadius, m_TargetRadius, _Time + offset);
		
		m_Indicator.Radius    = Mathf.Max(minRadius, maxRadius);
		m_Indicator.Thickness = Mathf.Abs(minRadius - maxRadius);
	}

	public override void Complete(Action _Callback = null)
	{
		if (m_Complete)
			return;
		
		m_Complete         = true;
		m_CompleteCallback = _Callback;
		
		if (gameObject.activeInHierarchy)
			StartCoroutine(CompleteRoutine(0.2f));
		else
			InvokeCompleteCallback();
	}

	public override void Success(Action _Callback = null)
	{
		Complete(_Callback);
	}

	public override void Fail(Action _Callback = null)
	{
		Complete(_Callback);
	}

	IEnumerator CompleteRoutine(float _Duration)
	{
		float source = CanvasGroup.alpha;
		float target = 0;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		CanvasGroup.alpha = target;
		
		InvokeCompleteCallback();
	}

	void InvokeCompleteCallback()
	{
		Action callback = m_CompleteCallback;
		m_CompleteCallback = null;
		callback?.Invoke();
	}
}