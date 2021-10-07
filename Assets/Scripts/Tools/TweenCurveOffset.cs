using UnityEngine;

public class TweenCurveOffset : Tween<float>
{
	[SerializeField] UISplineCurve m_Curve;

	protected override void Process(float _Phase)
	{
		if (m_Curve == null)
			return;
		
		m_Curve.Offset = Mathf.Lerp(Source, Target, _Phase);
	}
}