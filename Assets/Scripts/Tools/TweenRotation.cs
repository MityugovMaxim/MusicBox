using UnityEngine;

public class TweenRotation : Tween<float>
{
	protected override void Process(float _Phase)
	{
		RectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(Source, Target, _Phase));
	}
}