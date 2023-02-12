using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UISpline))]
public class UISplineChamfer : MonoBehaviour
{
	public enum Mode
	{
		Inner,
		Outer,
		Center,
	}

	const double WEIGHT = 0.582284749831;

	UISpline Spline
	{
		get
		{
			if (m_Spline == null)
				m_Spline = GetComponent<UISpline>();
			return m_Spline;
		}
	}

	[SerializeField] Mode    m_Mode;
	[SerializeField] float   m_Width;
	[SerializeField] Vector4 m_Corners;

	UISpline m_Spline;

	#if UNITY_EDITOR
	void OnValidate()
	{
		if (!gameObject.scene.isLoaded || Application.isPlaying)
			return;
		
		ProcessChamfer();
	}
	#endif

	void ProcessChamfer()
	{
		Vector2 pivot = Spline.RectTransform.pivot;
		
		List<UISpline.Key> keys = new List<UISpline.Key>();
		
		Vector2 tl = new Vector2(-pivot.x, pivot.y);
		Vector2 tr = new Vector2(pivot.x, pivot.y);
		Vector2 bl = new Vector2(-pivot.x, -pivot.y);
		Vector2 br = new Vector2(pivot.x, -pivot.y);
		
		Vector4 corners = GetCorners();
		
		ProcessCorner(tl, new Vector2(1, -1), corners.x, false, keys);
		ProcessCorner(tr, new Vector2(-1, -1), corners.y, true, keys);
		ProcessCorner(br, new Vector2(-1, 1), corners.w, false, keys);
		ProcessCorner(bl, new Vector2(1, 1), corners.z, true, keys);
		
		Spline.ClearKeys();
		Spline.Threshold = 20;
		Spline.Loop      = true;
		Spline.Optimize  = true;
		Spline.Uniform   = true;
		Spline.AddKeys(keys);
	}

	void ProcessCorner(Vector2 _Anchor, Vector2 _Direction, float _Value, bool _Inverse, List<UISpline.Key> _Keys)
	{
		float margin = GetMargin();
		
		Vector2 direction = _Direction * _Value;
		
		UISpline.Key a = new UISpline.Key();
		a.Anchor     = _Anchor;
		a.Position   = new Vector2(0, direction.y) + new Vector2(_Direction.x * margin, 0);
		
		UISpline.Key b = new UISpline.Key();
		b.Anchor     = _Anchor;
		b.Position   = new Vector2(direction.x, 0) + new Vector2(0, _Direction.y * margin);
		
		Vector2 tangents   = new Vector2((float)(direction.x * WEIGHT), (float)(direction.y * WEIGHT));
		Vector2 vertical   = new Vector2(0, tangents.y);
		Vector2 horizontal = new Vector2(tangents.x, 0);
		
		if (_Inverse)
		{
			a.InTangent  = -vertical;
			a.OutTangent = vertical;
			b.InTangent  = horizontal;
			b.OutTangent = -horizontal;
			_Keys.Add(b);
			_Keys.Add(a);
		}
		else
		{
			a.InTangent  = vertical;
			a.OutTangent = -vertical;
			b.InTangent  = -horizontal;
			b.OutTangent = horizontal;
			_Keys.Add(a);
			_Keys.Add(b);
		}
	}

	float GetMargin()
	{
		float mode;
		switch (m_Mode)
		{
			case Mode.Inner:
				mode = 0.5f;
				break;
			case Mode.Outer:
				mode = -0.5f;
				break;
			default:
				mode = 0;
				break;
		}
		
		return mode * m_Width;
	}

	Vector4 GetCorners()
	{
		float margin = GetMargin();
		
		Rect rect = Spline.GetLocalRect();
		
		Vector4 corners = m_Corners + new Vector4(margin, margin, margin, margin);
		
		float tlCorner = corners.x;
		if (tlCorner < 0)
			tlCorner = 0;
		
		float trCorner = corners.y;
		if (trCorner < 0)
			trCorner = 0;
		
		float blCorner = corners.z;
		if (blCorner < 0)
			blCorner = 0;
		
		float brCorner = corners.w;
		if (brCorner < 0)
			brCorner = 0;
		
		float top = Mathf.Max(0, corners.x + corners.y);
		if (top > float.Epsilon && top > rect.width)
		{
			tlCorner = corners.x / top * rect.width;
			trCorner = corners.y / top * rect.width;
		}
		
		float bottom = Mathf.Max(0, corners.z + corners.w);
		if (bottom > float.Epsilon && bottom > rect.width)
		{
			blCorner = corners.z / bottom * rect.width;
			brCorner = corners.w / bottom * rect.width;
		}
		
		float left = Mathf.Max(0, corners.x + corners.z);
		if (left > float.Epsilon && left > rect.height)
		{
			tlCorner = corners.x / left * rect.height;
			blCorner = corners.z / left * rect.height;
		}
		
		float right = Mathf.Max(0, corners.y + corners.w);
		if (right > float.Epsilon && right > rect.height)
		{
			trCorner = corners.y / right * rect.height;
			brCorner = corners.w / right * rect.height;
		}
		
		return new Vector4(tlCorner, trCorner, blCorner, brCorner);
	}
}
