using UnityEngine;

public class AlphaClip : Clip
{
	public AnimationCurve AlphaCurve => m_AlphaCurve;

	[SerializeField] AnimationCurve m_AlphaCurve;

	CanvasGroup m_CanvasGroup;

	public void Initialize(Sequencer _Sequencer, CanvasGroup _CanvasGroup)
	{
		base.Initialize(_Sequencer);
		
		m_CanvasGroup = _CanvasGroup;
	}

	protected override void OnEnter(float _Time)
	{
		if (m_CanvasGroup != null)
			m_CanvasGroup.alpha = m_AlphaCurve.Evaluate(GetNormalizedTime(_Time));
	}

	protected override void OnUpdate(float _Time)
	{
		if (m_CanvasGroup != null)
			m_CanvasGroup.alpha = m_AlphaCurve.Evaluate(GetNormalizedTime(_Time));
	}

	protected override void OnExit(float _Time)
	{
		if (m_CanvasGroup != null)
			m_CanvasGroup.alpha = m_AlphaCurve.Evaluate(GetNormalizedTime(_Time));
	}
}