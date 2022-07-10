using UnityEngine;

public static class RectExtension
{
	public static float GetAspect(this Rect _Rect)
	{
		return !Mathf.Approximately(_Rect.height, 0) ? _Rect.width / _Rect.height : 1;
	}

	public static Vector4 ToVector(this Rect _Rect)
	{
		return new Vector4(_Rect.x, _Rect.y, _Rect.width, _Rect.height);
	}

	public static Bounds ToBounds(this Rect _Rect)
	{
		return new Bounds(_Rect.center, _Rect.size);
	}

	public static Rect Transform(this Rect _Rect, Transform _Source, Transform _Target)
	{
		Vector2 min = _Rect.min;
		Vector2 max = _Rect.max;
		
		min = _Source.TransformPoint(min);
		max = _Source.TransformPoint(max);
		
		min = _Target.InverseTransformPoint(min);
		max = _Target.InverseTransformPoint(max);
		
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}

	public static Rect VerticalPadding(this Rect _Rect, float _Padding)
	{
		return new Rect(
			_Rect.x,
			_Rect.y + _Padding,
			_Rect.width,
			_Rect.height - _Padding * 2
		);
	}

	public static Rect HorizontalPadding(this Rect _Rect, float _Padding)
	{
		return new Rect(
			_Rect.x + _Padding,
			_Rect.y,
			_Rect.width - _Padding * 2,
			_Rect.height
		);
	}

	public static Rect VerticalResize(this Rect _Rect, float _Size, float _Pivot)
	{
		return new Rect(
			_Rect.x,
			-_Size * _Pivot,
			_Rect.width,
			_Size
		);
	}

	public static Rect HorizontalResize(this Rect _Rect, float _Size, Vector2 _Pivot)
	{
		return new Rect(
			-_Size * _Pivot.x,
			_Rect.y,
			_Size,
			_Rect.height
		);
	}
}