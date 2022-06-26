using System.Linq;
using UnityEngine;

public class UIPulseSpectrum : UISpectrum
{
	[SerializeField, Range(0, 1)] float m_Threshold = 0.25f;
	[SerializeField]              float m_MinScale  = 1;
	[SerializeField]              float m_MaxScale  = 1.5f;

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		float amplitude = _Amplitude.Average();
		
		float phase = Mathf.InverseLerp(m_Threshold, 0.9f, amplitude);
		
		float scale = EaseFunction.EaseOutQuad.Get(m_MinScale, m_MaxScale, phase);
		
		RectTransform.localScale = new Vector3(scale, scale, 1);
	}
}