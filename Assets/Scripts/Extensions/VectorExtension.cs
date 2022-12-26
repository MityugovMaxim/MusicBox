using UnityEngine;

public static class VectorExtension
{
	public static Vector4 ToVector(this RectOffset _Offset)
	{
		if (_Offset == null)
			return Vector4.zero;
		
		return new Vector4(
			_Offset.left,
			_Offset.right,
			_Offset.top,
			_Offset.bottom
		);
	}

	public static Quaternion ToRotation(this Vector2 _Vector, float _Offset = 0)
	{
		float angle = Vector2.SignedAngle(_Vector, Vector2.left);
		
		return Quaternion.Euler(0, 0, _Offset - angle);
	}

	public static Vector2 Rotate90(this Vector2 _Vector)
	{
		return new Vector2(-_Vector.y, _Vector.x);
	}

	public static Vector2 Rotate270(this Vector2 _Vector)
	{
		return new Vector2(_Vector.y, -_Vector.x);
	}

	public static Vector2 TransformPoint(this Vector2 _Vector, Transform _Source, Transform _Target)
	{
		Vector2 vector = _Source.TransformPoint(_Vector);
		
		return _Target.InverseTransformPoint(vector);
	}

	public static Vector2 TransformPoint(this Vector2 _Vector, Transform _Transform)
	{
		return _Transform.InverseTransformPoint(_Vector);
	}

	public static Vector2 VerticalClamp(this Vector2 _Vector, float _Min, float _Max)
	{
		_Vector.y = Mathf.Clamp(_Vector.y, _Min, _Max);
		return _Vector;
	}

	public static Vector2 HorizontalClamp(this Vector2 _Vector, float _Min, float _Max)
	{
		_Vector.x = Mathf.Clamp(_Vector.x, _Min, _Max);
		return _Vector;
	}
}
