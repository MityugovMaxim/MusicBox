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
}