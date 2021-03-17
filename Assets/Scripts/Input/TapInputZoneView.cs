using UnityEngine;

public class TapInputZoneView : InputZoneView
{
	[SerializeField] float m_Size;

	public override void Setup(float _Zone, float _ZoneMin, float _ZoneMax)
	{
		RectTransform.anchorMin = new Vector2(0.5f - m_Size * 0.5f, 0.5f - m_Size * 0.5f);
		RectTransform.anchorMax = new Vector2(0.5f + m_Size * 0.5f, 0.5f + m_Size * 0.5f);
	}
}
