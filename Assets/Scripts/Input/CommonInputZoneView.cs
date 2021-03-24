using UnityEngine;

public class CommonInputZoneView : InputZoneView
{
	[SerializeField]              Indicator m_Indicator;
	[SerializeField, Range(0, 1)] float     m_SourceRadius;
	[SerializeField, Range(0, 1)] float     m_TargetRadius;
	[SerializeField, Range(0, 1)] float     m_ZoneMin;
	[SerializeField, Range(0, 1)] float     m_ZoneMax;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		float minRadius = Mathf.Lerp(m_SourceRadius, m_TargetRadius, m_ZoneMin);
		float maxRadius = Mathf.Lerp(m_SourceRadius, m_TargetRadius, m_ZoneMax);
		
		m_Indicator.Radius    = Mathf.Max(minRadius, maxRadius);
		m_Indicator.Thickness = Mathf.Abs(minRadius - maxRadius);
	}
	#endif
}