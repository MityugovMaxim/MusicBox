using UnityEngine;

public static class TransformExtension
{
	public static Rect GetWorldRect(this RectTransform _RectTransform)
	{
		return _RectTransform.TransformRect(_RectTransform.rect);
	}

	public static Rect GetLocalRect(this RectTransform _RectTransform, Transform _Space)
	{
		return _Space.InverseTransformRect(_RectTransform.GetWorldRect());
	}

	public static bool Intersects(this RectTransform _RectTransform, RectTransform _Target)
	{
		return _Target.rect.Overlaps(_RectTransform.GetLocalRect(_Target));
	}

	public static bool Intersects(this RectTransform _RectTransform, RectTransform _Target, RectOffset _Padding)
	{
		return _Padding.Add(_Target.rect).Overlaps(_RectTransform.GetLocalRect(_Target));
	}

	public static Rect TransformRect(this Transform _Transform, Rect _Rect)
	{
		Vector2 min = _Transform.TransformPoint(_Rect.min);
		Vector2 max = _Transform.TransformPoint(_Rect.max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}

	public static Rect InverseTransformRect(this Transform _Transform, Rect _Rect)
	{
		Vector2 min = _Transform.InverseTransformPoint(_Rect.min);
		Vector2 max = _Transform.InverseTransformPoint(_Rect.max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}

	public static float GetVerticalAnchor(this RectTransform _Target, RectTransform _Source, float _Pivot)
	{
		Rect source = _Source.TransformRect(_Source.rect);
		Rect target = _Target.TransformRect(_Target.rect);
		
		float position = source.y + source.height * _Pivot;
		
		return MathUtility.Remap01(position, target.yMin, target.yMax);
	}

	public static float GetHorizontalAnchor(this RectTransform _Target, RectTransform _Source, float _Pivot)
	{
		Rect source = _Source.TransformRect(_Source.rect);
		Rect target = _Target.TransformRect(_Target.rect);
		
		float position = source.x + source.width * _Pivot;
		
		return MathUtility.Remap01(position, target.yMin, target.yMax);
	}

	public static Vector2 GetAnchor(this RectTransform _Target, RectTransform _Source, Vector2 _Pivot)
	{
		Rect source = _Source.TransformRect(_Source.rect);
		Rect target = _Target.TransformRect(_Target.rect);
		
		Vector2 position = source.position + Vector2.Scale(source.size, _Pivot);
		
		return new Vector2(
			MathUtility.Remap01(position.x, target.xMin, target.xMax),
			MathUtility.Remap01(position.y, target.yMin, target.yMax)
		);
	}
}