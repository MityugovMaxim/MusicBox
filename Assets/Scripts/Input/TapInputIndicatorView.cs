using System.Collections;
using UnityEngine;

public class TapInputIndicatorView : InputIndicatorView
{
	[SerializeField] CircleImage    m_Indicator;
	[SerializeField] float          m_SourceRadius;
	[SerializeField] float          m_TargetRadius;
	[SerializeField] AnimationCurve m_AlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);

	public override void Process(float _Time)
	{
		CanvasGroup.alpha = m_AlphaCurve.Evaluate(_Time);
		
		m_Indicator.Radius = Mathf.Lerp(m_SourceRadius, m_TargetRadius, _Time);
	}

	public override void Success()
	{
		StartCoroutine(ProcessRoutine(new Color(0, 1, 0, 0)));
	}

	public override void Fail()
	{
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
	}
}