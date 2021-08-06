using UnityEngine;

public static class MathUtility
{
	public static float Remap(float _Value, float _Low1, float _High1, float _Low2, float _High2)
	{
		return _Low2 + (_Value - _Low1) * (_High2 - _Low2) / (_High1 - _Low1);
	}

	public static float RemapClamped(float _Value, float _Low1, float _High1, float _Low2, float _High2)
	{
		return Mathf.Clamp(Remap(_Value, _Low1, _High1, _Low2, _High2), _Low2, _High2);
	}

	public static float Remap01(float _Value, float _Low, float _High)
	{
		return (_Value - _Low) / (_High - _Low);
	}

	public static float Remap01Clamped(float _Value, float _Low, float _High)
	{
		return Mathf.Clamp01(Remap01(_Value, _Low, _High));
	}

	public static float Snap(float _Value, float _Step)
	{
		return Mathf.Round(_Value / _Step) * _Step;
	}

	public static float Snap(float _Value, float _Min, float _Max, params float[] _Steps)
	{
		if (_Steps.Length == 0)
			return _Value;
		
		const float threshold = 200;
		
		float length = _Max - _Min;
		
		int index = -1;
		
		for (int i = _Steps.Length - 1; i >= 0; i--)
		{
			int count = (int)(length / _Steps[i]);
			
			if (count > threshold)
				break;
			
			index = i;
		}
		
		return index >= 0 && index < _Steps.Length ? Snap(_Value, _Steps[index]) : _Value;
	}

	public static Rect Round(Rect _Rect)
	{
		return new Rect(
			Mathf.Round(_Rect.x),
			Mathf.Round(_Rect.y),
			Mathf.Round(_Rect.width),
			Mathf.Round(_Rect.height)
		);
	}

	public static Rect Fit(Rect _Rect, float _Aspect)
	{
		return Fit(_Rect, _Aspect, new Vector2(0.5f, 0.5f));
	}

	public static Rect Fit(Rect _Rect, float _Aspect, Vector2 _Pivot)
	{
		Vector2 h = new Vector2(_Rect.width, _Rect.width / _Aspect);
		Vector2 v = new Vector2(_Rect.height * _Aspect, _Rect.height);
		
		Vector2 size     = h.x * h.y <= v.x * v.y ? h : v;
		Vector2 position = _Rect.position + Vector2.Scale(_Rect.size - size, _Pivot);
		
		return new Rect(position, size);
	}

	public static Rect Fill(Rect _Rect, float _Aspect)
	{
		return Fill(_Rect, _Aspect, new Vector2(0.5f, 0.5f));
	}

	public static Rect Fill(Rect _Rect, float _Aspect, Vector2 _Pivot)
	{
		Vector2 h = new Vector2(_Rect.width, _Rect.width / _Aspect);
		Vector2 v = new Vector2(_Rect.height * _Aspect, _Rect.height);
		
		Vector2 size     = h.x * h.y >= v.x * v.y ? h : v;
		Vector2 position = _Rect.position + Vector2.Scale(_Rect.size - size, _Pivot);
		
		return new Rect(position, size);
	}

	public static Rect Uniform(Rect _Source, Rect _Target)
	{
		Rect rect = new Rect(
			Remap01(_Source.x, _Target.xMin, _Target.xMax),
			Remap01(_Source.y, _Target.yMin, _Target.yMax),
			_Source.width / _Target.width,
			_Source.height / _Target.height
		);
		
		return Fit(rect, _Target.width / _Target.height);
	}

	public static int Repeat(int _Value, int _Length)
	{
		int value = _Value % _Length;
		return value < 0 ? value + _Length : value;
	}
}