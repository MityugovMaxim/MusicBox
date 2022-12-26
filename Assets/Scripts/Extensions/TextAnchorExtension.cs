using UnityEngine;

public static class TextAnchorExtension
{
	public static float GetVerticalPivot(this TextAnchor _Alignment)
	{
		switch (_Alignment)
		{
			case TextAnchor.UpperLeft:
			case TextAnchor.UpperCenter:
			case TextAnchor.UpperRight:
				return 1.0f;
			case TextAnchor.MiddleLeft:
			case TextAnchor.MiddleCenter:
			case TextAnchor.MiddleRight:
				return 0.5f;
			case TextAnchor.LowerLeft:
			case TextAnchor.LowerCenter:
			case TextAnchor.LowerRight:
				return 0.0f;
			default:
				return 0.0f;
		}
	}

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
