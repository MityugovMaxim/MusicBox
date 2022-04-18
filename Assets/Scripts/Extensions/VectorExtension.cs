using UnityEngine;

public static class VectorExtension
{
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
}