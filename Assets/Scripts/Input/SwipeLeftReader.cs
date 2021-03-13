using UnityEditor;
using UnityEngine;

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

	RectTransform m_RectTransform;

	void OnDrawGizmos()
	{
		Matrix4x4 modelMatrix = RectTransform.localToWorldMatrix;
		
		Handles.matrix = modelMatrix;
		Gizmos.matrix  = modelMatrix;
		
		Rect rect = RectTransform.rect;
		
		Rect aRect = new Rect(
			rect.x,
			rect.y,
			rect.width * SuccessTime,
			rect.height
		);
		
		Rect bRect = new Rect(
			rect.x + rect.width * SuccessTime,
			rect.y,
			rect.width * (PerfectTime - SuccessTime),
			rect.height
		);
		
		Rect cRect = new Rect(
			rect.x + rect.width * PerfectTime,
			rect.y,
			rect.width * (1 - PerfectTime),
			rect.height
		);
		
		Handles.DrawSolidRectangleWithOutline(
			aRect,
			new Color(0.86f, 0.31f, 0.33f, 0.2f),
			Color.clear
		);
		
		Handles.DrawSolidRectangleWithOutline(
			bRect,
			new Color(1f, 0.71f, 0f, 0.2f),
			Color.clear
		);
		
		Handles.DrawSolidRectangleWithOutline(
			cRect,
			new Color(0, 0.8f, 0.7f, 0.2f),
			Color.clear
		);
		
		Handles.matrix = Matrix4x4.identity;
		Gizmos.matrix  = Matrix4x4.identity;
	}

	protected override void Success()
	{
	}

	protected override void Fail()
	{
	}
}