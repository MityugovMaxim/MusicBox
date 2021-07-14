using UnityEngine;

public static class RectExtension
{
	public static Vector4 ToVector(this Rect _Rect)
	{
		return new Vector4(_Rect.x, _Rect.y, _Rect.width, _Rect.height);
	}
}