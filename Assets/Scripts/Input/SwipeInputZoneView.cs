using UnityEngine;

public class SwipeInputZoneView : InputZoneView
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down,
	}

	[SerializeField] Direction m_Direction;
	[SerializeField] float     m_Size = 0.1f;

	public override void Setup(float _Zone, float _ZoneMin, float _ZoneMax)
	{
		Vector2 anchorMin = RectTransform.anchorMin;
		Vector2 anchorMax = RectTransform.anchorMax;
		
		float min = _Zone - m_Size * 0.5f;
		float max = _Zone + m_Size * 0.5f;
		
		switch (m_Direction)
		{
			case Direction.Left:
				anchorMin.x = min;
				anchorMax.x = max;
				break;
			case Direction.Right:
				anchorMin.x = 1 - max;
				anchorMax.x = 1 - min;
				break;
			case Direction.Up:
				anchorMin.y = 1 - max;
				anchorMax.y = 1 - min;
				break;
			case Direction.Down:
				anchorMin.y = min;
				anchorMax.y = max;
				break;
		}
		
		RectTransform.anchorMin = anchorMin;
		RectTransform.anchorMax = anchorMax;
	}
}