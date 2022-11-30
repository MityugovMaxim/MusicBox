using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIEntity : UIBehaviour
{
	public Transform Transform
	{
		get
		{
			if (ReferenceEquals(m_Transform, null))
				m_Transform = GetComponent<Transform>();
			return m_Transform;
		}
	}

	public RectTransform RectTransform
	{
		get
		{
			if (ReferenceEquals(m_RectTransform, null))
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	public bool IsActive => gameObject.activeSelf;

	protected bool IsInstanced => gameObject.scene.isLoaded;

	[NonSerialized] RectTransform m_RectTransform;
	[NonSerialized] Transform     m_Transform;

	public void BringToFront()
	{
		RectTransform.SetAsLastSibling();
	}

	public void BringToBack()
	{
		RectTransform.SetAsFirstSibling();
	}

	public void SetRect(Rect _Rect)
	{
		Vector2 anchor = new Vector2(0, 1);
		Vector2 pivot  = RectTransform.pivot;
		
		RectTransform.anchorMin = anchor;
		RectTransform.anchorMax = anchor;
		
		RectTransform.anchoredPosition = new Vector2(
			_Rect.x + _Rect.width * pivot.x,
			_Rect.y - _Rect.height * pivot.y
		);
		
		RectTransform.sizeDelta = _Rect.size;
	}

	public Vector2 GetLocalPoint(Vector2 _Position, Camera _Camera)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			RectTransform,
			_Position,
			_Camera,
			out Vector2 position
		);
		
		return position;
	}

	public Vector2 GetLocalPoint(Vector2 _Point)
	{
		return RectTransform.InverseTransformPoint(_Point);
	}

	public Vector2 GetWorldPoint()
	{
		return RectTransform.TransformPoint(RectTransform.rect.center);
	}

	public Vector2 GetWorldPoint(Vector2 _Point)
	{
		return RectTransform.TransformPoint(_Point);
	}

	public Rect GetLocalRect()
	{
		return RectTransform.rect;
	}

	public Rect GetLocalRect(RectOffset _Margin)
	{
		return _Margin.Add(RectTransform.rect);
	}

	public Rect GetLocalRect(Rect _Rect)
	{
		return RectTransform.InverseTransformRect(_Rect);
	}

	public Rect GetWorldRect()
	{
		return RectTransform.GetWorldRect();
	}

	public Rect GetWorldRect(RectOffset _Margin)
	{
		return RectTransform.TransformRect(GetLocalRect(_Margin));
	}

	public Rect GetWorldRect(Rect _Rect)
	{
		return RectTransform.TransformRect(_Rect);
	}

	public bool Overlaps(RectTransform _RectTransform)
	{
		Rect source = RectTransform.GetWorldRect();
		Rect target = _RectTransform.GetWorldRect();
		return source.Overlaps(target);
	}

	public bool Overlaps(UIEntity _Entity)
	{
		Rect source = GetWorldRect();
		Rect target = _Entity.GetWorldRect();
		return source.Overlaps(target);
	}
}
