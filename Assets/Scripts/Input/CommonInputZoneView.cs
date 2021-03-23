using UnityEngine;

public class CommonInputZoneView : InputZoneView
{
	[SerializeField] Indicator m_Indicator;
	[SerializeField] float     m_SourceRadius;
	[SerializeField] float     m_TargetRadius;
	[SerializeField] float     m_Size = 0.1f;

	public override void Setup(float _Zone, float _ZoneMin, float _ZoneMax)
	{
		m_Indicator.Radius    = Mathf.Lerp(m_SourceRadius, m_TargetRadius, _Zone) + m_Size * 0.5f;
		m_Indicator.Thickness = m_Size;
	}
}