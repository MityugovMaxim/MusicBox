using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InputZoneView : UIBehaviour
{
	protected RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	RectTransform m_RectTransform;

	public abstract void Setup(float _Zone, float _ZoneMin, float _ZoneMax);
}