using UnityEngine;
using UnityEngine.UI;

public class TweenAlpha : Tween<float>
{
	[SerializeField] Graphic m_Graphic;

	protected override void Process(float _Phase)
	{
		if (m_Graphic == null)
			return;
		
		Color color = m_Graphic.color;
		color.a        = Mathf.Lerp(Source, Target, _Phase);
		m_Graphic.color = color;
	}
}