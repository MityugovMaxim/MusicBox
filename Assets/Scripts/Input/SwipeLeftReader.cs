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

	// void OnDrawGizmos()
	// {
	// 	Matrix4x4 modelMatrix = RectTransform.localToWorldMatrix;
	// 	
	// 	Handles.matrix = modelMatrix;
	// 	Gizmos.matrix  = modelMatrix;
	// 	
	// 	Rect rect = RectTransform.rect;
	// 	
	// 	Rect failRect = new Rect(
	// 		rect.x,
	// 		rect.y,
	// 		rect.width * SuccessRange.Min,
	// 		rect.height
	// 	);
	// 	
	// 	Rect bRect = new Rect(
	// 		rect.x + rect.width * SuccessRange,
	// 		rect.y,
	// 		rect.width * (PerfectRange - SuccessRange),
	// 		rect.height
	// 	);
	// 	
	// 	Rect cRect = new Rect(
	// 		rect.x + rect.width * PerfectRange,
	// 		rect.y,
	// 		rect.width * (1 - PerfectRange),
	// 		rect.height
	// 	);
	// 	
	// 	Handles.DrawSolidRectangleWithOutline(
	// 		failRect,
	// 		new Color(0.86f, 0.31f, 0.33f, 0.2f),
	// 		Color.clear
	// 	);
	// 	
	// 	Handles.DrawSolidRectangleWithOutline(
	// 		bRect,
	// 		new Color(1f, 0.71f, 0f, 0.2f),
	// 		Color.clear
	// 	);
	// 	
	// 	Handles.DrawSolidRectangleWithOutline(
	// 		cRect,
	// 		new Color(0, 0.8f, 0.7f, 0.2f),
	// 		Color.clear
	// 	);
	// 	
	// 	Handles.matrix = Matrix4x4.identity;
	// 	Gizmos.matrix  = Matrix4x4.identity;
	// }

	public override void UpdateRoutine(float _Time)
	{
		base.UpdateRoutine(_Time);
		
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

	public override void FinishRoutine(float _Time)
	{
		base.FinishRoutine(_Time);
		
		Color color = m_Indicator.color;
		color.a = 0;
		m_Indicator.color = color;
	}

	protected override void Success()
	{
	}

	protected override void Fail()
	{
	}
}