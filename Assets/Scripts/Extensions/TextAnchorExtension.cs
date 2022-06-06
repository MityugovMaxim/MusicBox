using UnityEngine;

public static class TextAnchorExtension
{
	public static Vector2 GetPivot(this TextAnchor _Alignment)
	{
		switch (_Alignment)
		{
			case TextAnchor.UpperLeft:
				return new Vector2(0, 1);
			case TextAnchor.UpperCenter:
				return new Vector2(0.5f, 1);
			case TextAnchor.UpperRight:
				return new Vector2(1, 1);
			case TextAnchor.MiddleLeft:
				return new Vector2(0, 0.5f);
			case TextAnchor.MiddleCenter:
				return new Vector2(0.5f, 0.5f);
			case TextAnchor.MiddleRight:
				return new Vector2(1, 0.5f);
			case TextAnchor.LowerLeft:
				return new Vector2(0, 0);
			case TextAnchor.LowerCenter:
				return new Vector2(0.5f, 0);
			case TextAnchor.LowerRight:
				return new Vector2(1, 0);
			default:
				return new Vector2(0, 0);
		}
	}
}