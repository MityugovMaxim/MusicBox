using UnityEngine;

public class TweenAnchoredPosition : Tween<Vector2>
{
	protected override void Process(float _Phase)
	{
		RectTransform.anchoredPosition = Vector2.Lerp(Source, Target, _Phase);
	}
}
