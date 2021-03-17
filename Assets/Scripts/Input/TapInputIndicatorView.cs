using UnityEngine;

public class TapInputIndicatorView : InputIndicatorView
{
	[SerializeField] Vector3        m_SourceScale;
	[SerializeField] Vector3        m_TargetScale;
	[SerializeField] AnimationCurve m_AlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);

	public override void Process(float _Time)
	{
		RectTransform.localScale = Vector3.LerpUnclamped(m_SourceScale, m_TargetScale, _Time);
		
		CanvasGroup.alpha = m_AlphaCurve.Evaluate(_Time);
	}

	public override void Success()
	{
	}

	public override void Fail()
	{
	}
}