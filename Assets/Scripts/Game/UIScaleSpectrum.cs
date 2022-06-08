using System.Linq;
using UnityEngine;

public class UIScaleSpectrum : UISpectrum
{
	[SerializeField] float m_MinScale = 1;
	[SerializeField] float m_MaxScale = 1.5f;

	public override void Reposition() { }

	public override void Sample(float[] _Buffer)
	{
		if (enabled)
		{
			float phase = _Buffer.Sum() / _Buffer.Length;
			
			float scale = Mathf.Lerp(m_MinScale, m_MaxScale, phase);
			
			RectTransform.localScale = new Vector3(scale, scale, 1);
		}
		else
		{
			RectTransform.localScale = new Vector3(m_MinScale, m_MinScale, 1);
		}
	}
}
