using System.Linq;
using UnityEngine;

public class UIScaleSpectrum : UISpectrum
{
	[SerializeField, Range(0, 1)] float m_Threshold;

	[SerializeField] RectTransform m_Transform;
	[SerializeField] Vector3       m_MinScale;
	[SerializeField] Vector3       m_MaxScale;
	[SerializeField] float         m_AttackDamp = 0.6f;
	[SerializeField] float         m_DecayDamp  = 0.1f;

	float m_Phase;

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		float amplitude = _Amplitude.Average();
		
		float phase = Mathf.InverseLerp(m_Threshold, 0.9f, amplitude);
		
		Vector3 source = m_Transform.localScale;
		Vector2 target = Vector3.LerpUnclamped(m_MinScale, m_MaxScale, phase);
		
		float damp = m_Phase < phase ? m_AttackDamp : m_DecayDamp;
		
		m_Phase = Mathf.Lerp(m_Phase, phase, damp);
		
		m_Transform.localScale = Vector2.LerpUnclamped(source, target, m_Phase);
	}
}