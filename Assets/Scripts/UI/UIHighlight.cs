using UnityEngine;

public class UIHighlight : UIGroup
{
	[SerializeField] UISplineCurve m_Curve;
	[SerializeField] float         m_Speed;

	void Update()
	{
		m_Curve.Offset = -Mathf.Repeat(Time.realtimeSinceStartup * m_Speed, 1);
	}
}
