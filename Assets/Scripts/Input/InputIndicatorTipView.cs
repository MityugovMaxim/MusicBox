using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputIndicatorTipView : UIBehaviour
{
	[SerializeField] Graphic        m_Graphic;
	[SerializeField] float          m_MinTime = 0;
	[SerializeField] float          m_MaxTime = 1;
	[SerializeField] AnimationCurve m_Curve   = AnimationCurve.Linear(0, 0, 1, 1);

	public void Process(float _Time)
	{
		if (m_Graphic == null)
			return;
		
		float time = MathUtility.Remap01Clamped(_Time, m_MinTime, m_MaxTime);
		
		Color color = m_Graphic.color;
		color.a         = m_Curve.Evaluate(time);
		m_Graphic.color = color;
	}
}