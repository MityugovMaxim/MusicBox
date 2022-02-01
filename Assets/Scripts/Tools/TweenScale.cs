using UnityEngine;

public class TweenScale : Tween<Vector3>
{
	protected override void Process(float _Phase)
	{
		RectTransform.localScale = Vector3.LerpUnclamped(Source, Target, _Phase);
	}
}