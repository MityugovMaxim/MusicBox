using UnityEngine;

public static class SplineExtension
{
	public static float GetHorizontalDistance(this UISpline _Spline, Vector2 _Position)
	{
		int index = FindVerticalAnchor(_Spline, _Position.y);
		
		if (index < 0)
			return float.MaxValue;
		
		UISpline.Point point = _Spline[index];
		
		return Mathf.Abs(_Position.x - point.Position.x);
	}

	static int FindVerticalAnchor(UISpline _Spline, float _Position)
	{
		int i = 0;
		int j = _Spline.Length - 1;
		int k = -1;
		while (i <= j)
		{
			k = (i + j) / 2;
			UISpline.Point point = _Spline[k];
			if (point.Position.y < _Position)
				i = k + 1;
			else if (point.Position.y > _Position)
				j = k - 1;
			else
				break;
		}
		return k;
	}
}