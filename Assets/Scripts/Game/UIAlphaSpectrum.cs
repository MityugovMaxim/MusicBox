using System.Linq;
using UnityEngine;

public class UIAlphaSpectrum : UISpectrum
{
	[SerializeField, Range(0, 1)] float    m_Threshold = 0.3f;
	[SerializeField]              UISprite m_Graphic;
	[SerializeField]              float    m_MinAlpha;
	[SerializeField]              float    m_MaxAlpha;
	[SerializeField]              float    m_AttackDamp = 0.6f;
	[SerializeField]              float    m_DecayDamp  = 0.1f;

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		float amplitude = _Amplitude.Average();
		
		float phase = Mathf.InverseLerp(m_Threshold, 0.9f, amplitude);
		
		float source = m_Graphic.Alpha;
		float target = EaseFunction.EaseOutQuad.Get(m_MinAlpha, m_MaxAlpha, phase);
		
		float alpha = source < target
			? Mathf.Lerp(source, target, m_AttackDamp)
			: Mathf.Lerp(source, target, m_DecayDamp);
		
		m_Graphic.Alpha = alpha;
	}
}