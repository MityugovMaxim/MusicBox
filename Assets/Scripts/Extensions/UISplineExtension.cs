using System.Linq;
using UnityEngine;

public static class UISplineExtension
{
	public static float EvaluateVertical(this UISpline _Spline, float _Phase)
	{
		float position = Mathf.Lerp(
			_Spline.First().Position.y,
			_Spline.Last().Position.y,
			_Phase
		);
		
		int i = 0;
		int j = _Spline.Length - 1;
		if (_Spline.Length > 2)
		{
			while (i < j)
			{
				int k = (i + j) / 2;
				
				float value = _Spline[k].Position.y;
				
				if (value > position)
					j = k;
				else
					i = k;
				
				if (j - i <= 1)
					break;
			}
		}
		
		float sourceLength = _Spline[i].Position.y;
		float targetLength = _Spline[j].Position.y;
		
		return Mathf.LerpUnclamped(
			_Spline[i].Phase,
			_Spline[j].Phase,
			Mathf.InverseLerp(sourceLength, targetLength, position)
		);
	}
}