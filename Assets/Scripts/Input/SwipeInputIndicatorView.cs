using UnityEngine;

public class SwipeInputIndicatorView : InputIndicatorView
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
	}

	public override void Fail()
	{
	}
}