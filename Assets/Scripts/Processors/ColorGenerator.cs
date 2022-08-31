using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Random = UnityEngine.Random;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public static class ColorGenerator
{
	public static Color GenerateColor(float _Alpha = 1.0f)
	{
		float h = Random.Range(0.0f, 1.0f);
		float s = Random.Range(0.5f, 1.0f);
		float v = Random.Range(0.5f, 1.0f);
		
		Color color = Color.HSVToRGB(h, s, v);
		
		color.a = _Alpha;
		
		return color;
	}

	public static Color[] Arbitrary(Color _Base)
	{
		return new Color[]
		{
			GenerateColor(),
			GenerateColor(),
			new Color(1, 1, 1, 0.75f),
			GenerateColor(),
		};
	}

	public static Color[] Monochromatic(Color _Base)
	{
		float s    = _Base.GetS();
		float step = -Random.Range(0.0f, 100.0f * s / 3.0f);
		
		(float a, float b) = RandomSwitch(step * 1, step * 2);
		
		return new Color[]
		{
			_Base,
			_Base.DevianceH(15).ShiftS(a),
			new Color(1, 1, 1, 0.75f),
			_Base.DevianceH(15).ShiftS(b),
		};
	}

	public static Color[] Complementary(Color _Base)
	{
		(Color a, Color b) = RandomSwitch(
			_Base.ShiftH(180).DevianceS(25),
			_Base.DevianceS(50)
		);
		
		return new Color[]
		{
			_Base,
			a,
			new Color(1, 1, 1, 0.75f),
			b,
		};
	}

	public static Color[] SplitComplementary(Color _Base)
	{
		(float a, float b) = RandomSwitch(150, 210);
		
		return new Color[]
		{
			_Base,
			_Base.ShiftH(a),
			new Color(1, 1, 1, 0.75f),
			_Base.ShiftH(b),
		};
	}

	public static Color[] Triad(Color _Base)
	{
		(float a, float b) = RandomSwitch(120, 240);
		
		return new Color[]
		{
			_Base,
			_Base.ShiftH(a).DevianceS(15),
			new Color(1, 1, 1, 0.75f),
			_Base.ShiftH(b).DevianceS(15),
		};
	}

	public static Color[] Square(Color _Base)
	{
		(float a, float b) = RandomSwitch(90, 270);
		
		return new Color[]
		{
			_Base,
			_Base.ShiftH(a).DevianceS(25),
			new Color(1, 1, 1, 0.75f),
			_Base.ShiftH(b).DevianceS(25),
		};
	}

	public static Color[] Analogous(Color _Base)
	{
		float angle = Random.Range(0.0f, 90.0f);
		
		(float a, float b) = RandomSwitch(angle, -angle);
		
		return new Color[]
		{
			_Base,
			_Base.ShiftH(a).ShiftS(5),
			new Color(1, 1, 1, 0.75f),
			_Base.ShiftH(b).ShiftS(5),
		};
	}

	public static Color[] Shades(Color _Base)
	{
		(float a, float b) = RandomSwitch(-50, -25);
		
		return new Color[]
		{
			_Base,
			_Base.ShiftV(a).DevianceH(15).DevianceS(25),
			new Color(1, 1, 1, 0.75f),
			_Base.ShiftV(b).DevianceH(15).DevianceS(25),
		};
	}

	public static Color[] GoldenRatio(Color _Base)
	{
		const float angle = 137.5f;
		
		(float a, float b) = RandomSwitch(angle * 1, angle * 2);
		
		return new Color[]
		{
			_Base,
			_Base.ShiftH(a),
			new Color(1, 1, 1, 0.75f),
			_Base.ShiftH(b),
		};
	}

	static float GetH(this Color _Color)
	{
		Color.RGBToHSV(_Color, out float h, out _, out _);
		
		return h;
	}

	static float GetS(this Color _Color)
	{
		Color.RGBToHSV(_Color, out _, out float s, out _);
		
		return s;
	}

	static float GetV(this Color _Color)
	{
		Color.RGBToHSV(_Color, out _, out _, out float v);
		
		return v;
	}

	static Color ApplyHSV(this Color _Color, float _H, float _S, float _V)
	{
		Color color = Color.HSVToRGB(_H, _S, _V);
		
		color.a = _Color.a;
		
		return color;
	}

	static Color ShiftH(this Color _Color, float _Shift)
	{
		float shift = _Shift / 360.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		h = Mathf.Repeat(h + shift, 1.0f);
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color ShiftS(this Color _Color, float _Shift)
	{
		float shift = _Shift / 100.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		s = Mathf.Clamp01(s + shift);
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color ShiftV(this Color _Color, float _Shift)
	{
		float shift = _Shift / 100.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		v = Mathf.Clamp01(v + shift);
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color DevianceH(this Color _Color, float _Range)
	{
		float range = _Range / 360.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		h = Mathf.Clamp01(Random.Range(h - range, h + range));
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color DevianceS(this Color _Color, float _Range)
	{
		float range = _Range / 100.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		s = Mathf.Clamp01(Random.Range(s - range, s + range));
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color DevianceV(this Color _Color, float _Range)
	{
		float range = _Range / 100.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		v = Mathf.Clamp01(Random.Range(v - range, v + range));
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color RandomH(this Color _Color, float _Min, float _Max)
	{
		float min = _Min / 360.0f;
		float max = _Max / 360.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		h = Random.Range(min, max);
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color RandomS(this Color _Color, float _Min, float _Max)
	{
		float min = _Min / 100.0f;
		float max = _Max / 100.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		s = Random.Range(min, max);
		
		return _Color.ApplyHSV(h, s, v);
	}

	static Color RandomV(this Color _Color, float _Min, float _Max)
	{
		float min = _Min / 100.0f;
		float max = _Max / 100.0f;
		
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		v = Random.Range(min, max);
		
		return _Color.ApplyHSV(h, s, v);
	}

	static (T a, T b) RandomSwitch<T>(T _A, T _B)
	{
		float value = Random.value;
		return value >= 0.5f ? (_A, _B) : (_B, _A);
	}
}