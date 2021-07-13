using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIIndicatorZone : UIBehaviour
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

	[SerializeField] RectOffset m_Padding;

	RectTransform m_RectTransform;

	readonly List<UIIndicator> m_Inactive = new List<UIIndicator>();
	readonly List<UIIndicator> m_Active   = new List<UIIndicator>();

	public void Process()
	{
		// Enable input
		
		
		// Disable input
		
	}

	public void RegisterIndicator(UIIndicator _Indicator)
	{
		m_Inactive.Add(_Indicator);
	}

	public void UnregisterIndicator(UIIndicator _Indicator)
	{
		m_Inactive.Remove(_Indicator);
	}
}
