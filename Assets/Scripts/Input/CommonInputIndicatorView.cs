using System;
using System.Collections;
using UnityEngine;

public class CommonInputIndicatorView : InputIndicatorView
{
	[SerializeField] Indicator    m_Indicator;
	[SerializeField] float          m_SourceRadius;
	[SerializeField] float          m_TargetRadius;
	[SerializeField] AnimationCurve m_AlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);

	bool   m_Complete;
	Action m_CompleteCallback;

	public override void Process(float _Time)
	{
		if (m_Complete)
			return;
		
		CanvasGroup.alpha = m_AlphaCurve.Evaluate(_Time);
		
		m_Indicator.Radius = Mathf.Lerp(m_SourceRadius, m_TargetRadius, _Time);
	}

	public override void Complete(Action _Callback = null)
	{
		InvokeCompleteCallback();
		
		if (m_Complete)
			return;
		
		m_Complete = true;
		
		m_CompleteCallback = _Callback;
		
		InvokeCompleteCallback();
	}

	public override void Success(Action _Callback = null)
	{
		InvokeCompleteCallback();
		
		if (m_Complete)
			return;
		
		m_Complete = true;
		
		m_CompleteCallback = _Callback;
		
		StartCoroutine(ProcessRoutine(new Color(0, 1, 0, 0)));
	}

	public override void Fail(Action _Callback = null)
	{
		InvokeCompleteCallback();
		
		if (m_Complete)
			return;
		
		m_Complete = true;
		
		m_CompleteCallback = _Callback;
		
		StartCoroutine(ProcessRoutine(new Color(1, 0, 0, 0)));
	}

	IEnumerator ProcessRoutine(Color _Color)
	{
		Color source = m_Indicator.color;
		Color target = _Color;
		
		const float duration = 0.2f;
		
		float time = 0;
		while (time < duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			m_Indicator.color = Color.Lerp(source, target, time / duration);
		}
		
		m_Indicator.color = target;
		
		InvokeCompleteCallback();
	}

	void InvokeCompleteCallback()
	{
		Action callback = m_CompleteCallback;
		m_CompleteCallback = null;
		callback?.Invoke();
	}
}