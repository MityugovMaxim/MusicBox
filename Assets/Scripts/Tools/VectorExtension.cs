using UnityEngine;

public static class VectorExtension
{
	public static Vector2 Rotate90(this Vector2 _Vector)
	{
		return new Vector2(-_Vector.y, _Vector.x);
	}

	public static Vector2 Rotate180(this Vector2 _Vector)
	{
		return new Vector2(-_Vector.x, -_Vector.y);
	}

	public static Vector2 Rotate270(this Vector2 _Vector)
	{
		return new Vector2(_Vector.y, -_Vector.x);
	}
}