using System.Linq;
using UnityEngine;

public class UIPulseSpectrum : UISpectrum
{
	[SerializeField, Range(0, 1)] float m_Threshold = 0.4f;
	[SerializeField]              float m_MinScale  = 1;
	[SerializeField]              float m_MaxScale  = 1.5f;
	[SerializeField]              float m_AttackDamp = 0.6f;
	[SerializeField]              float m_DecayDamp = 0.1f;

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		float amplitude = _Amplitude.Average();
		
		float phase = Mathf.InverseLerp(m_Threshold, 0.9f, amplitude);
		
		float source = RectTransform.localScale.x;
		float target = EaseFunction.EaseOutQuad.Get(m_MinScale, m_MaxScale, phase);
		
		float scale = source < target
			? Mathf.Lerp(source, target, m_AttackDamp)
			: Mathf.Lerp(source, target, m_DecayDamp);
		
		RectTransform.localScale = new Vector3(scale, scale, 1);
	}
}