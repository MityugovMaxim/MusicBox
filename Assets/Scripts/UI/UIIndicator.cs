using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIIndicator : UIBehaviour
{
	public RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	public abstract UIHandle Handle { get; }

	public abstract float MinPadding { get; }
	public abstract float MaxPadding { get; }

	RectTransform m_RectTransform;
}