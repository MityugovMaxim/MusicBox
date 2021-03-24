using System;
using System.Collections;
using UnityEngine;

public class CommonInputIndicatorView : InputIndicatorView
{
	[SerializeField] Indicator             m_Indicator;
	[SerializeField] float                 m_SourceRadius;
	[SerializeField] float                 m_TargetRadius;
	[SerializeField] float                 m_Size = 0.1f;
	[SerializeField] InputIndicatorTipView m_Tip;
	[SerializeField] AnimationCurve        m_AlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);

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
		
		m_Indicator.Radius    = Mathf.Lerp(m_SourceRadius, m_TargetRadius, _Time) + m_Size * 0.5f;
		m_Indicator.Thickness = m_Size;
		
		if (m_Tip != null)
			m_Tip.Process(m_Indicator.Radius);
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