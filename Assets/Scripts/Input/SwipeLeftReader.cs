using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SwipeLeftReader : InputReader
{
	RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	[SerializeField] Image m_Indicator;

	RectTransform m_RectTransform;

	protected override void Begin()
	{
		m_Indicator.color = new Color(1, 1, 1, 0);
	}

	protected override void Process(float _Time)
	{
		RectTransform rectTransform = m_Indicator.rectTransform;
		
		Vector2 minAnchor = rectTransform.anchorMin;
		Vector2 maxAnchor = rectTransform.anchorMax;
		
		minAnchor.x = _Time;
		maxAnchor.x = _Time;
		
		rectTransform.anchorMin = minAnchor;
		rectTransform.anchorMax = maxAnchor;
		
		Color color = m_Indicator.color;
		color.a = MathUtility.Remap01(_Time, 0, 0.5f);
		m_Indicator.color = color;
	}

	protected override void Complete(float _Time)
	{
		Color color = m_Indicator.color;
		color.a           = 1 - _Time;
		m_Indicator.color = color;
	}

	protected override void Finish()
	{
		Color color = m_Indicator.color;
		color.a = 0;
		m_Indicator.color = color;
	}

	protected override void Success()
	{
		m_Indicator.color = new Color(0, 0.8f, 0.69f, m_Indicator.color.a);
	}

	protected override void Fail()
	{
		m_Indicator.color = new Color(1, 0.42f, 0, m_Indicator.color.a);
	}
}