using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEntity : UIBehaviour
{
	public RectTransform RectTransform
	{
		get
		{
			if ((object)m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	protected bool IsInstanced => gameObject.scene.isLoaded;

	[NonSerialized] RectTransform m_RectTransform;

	public void BringToFront()
	{
		RectTransform.SetAsLastSibling();
	}

	public void BringToBack()
	{
		RectTransform.SetAsFirstSibling();
	}

	public Vector2 GetLocalPoint(Vector2 _Point)
	{
		return RectTransform.InverseTransformPoint(_Point);
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