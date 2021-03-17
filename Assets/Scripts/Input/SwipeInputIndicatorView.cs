using UnityEngine;

public class SwipeInputIndicatorView : InputIndicatorView
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down,
	}

	[SerializeField] Direction      m_Direction;
	[SerializeField] float          m_Size       = 0.1f;
	[SerializeField] AnimationCurve m_AlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);

	public override void Process(float _Time)
	{
		Vector2 anchorMin = RectTransform.anchorMin;
		Vector2 anchorMax = RectTransform.anchorMax;
		
		float min = _Time - m_Size * 0.5f;
		float max = _Time + m_Size * 0.5f;
		
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
		
		CanvasGroup.alpha = m_AlphaCurve.Evaluate(_Time);
	}

	public override void Success()
	{
	}

	public override void Fail()
	{
	}
}