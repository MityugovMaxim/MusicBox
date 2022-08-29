using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ColorGenerator
{
	static readonly Func<Color, Color[]>[] m_Algorithms =
	{
		Monochromatic,
		Complementary,
		SplitComplementary,
		Triad,
		Square,
		Analogous,
		Shades,
		GoldenRatio,
	};

	public static Color[] Generate()
	{
		Color color = GenerateBase();
		
		int index = Random.Range(0, m_Algorithms.Length);
		
		return m_Algorithms[index].Invoke(color);
	}

	public static Color GenerateBase()
	{
		float h = Random.Range(0.0f, 1.0f);
		float s = Random.Range(0.5f, 1.0f);
		float v = Random.Range(0.5f, 1.0f);
		
		return Color.HSVToRGB(h, s, v);
	}

	public static Color[] Monochromatic(Color _Base)
	{
		Color[] colors = new Color[4];
		
		float s    = _Base.GetS();
		float step = -Random.Range(0.0f, 100.0f * s / 3.0f);
		
		colors[0] = _Base;
		colors[1] = _Base.DevianceH(5).ShiftS(step * 1);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[3] = _Base.DevianceH(5).ShiftS(step * 2);
		
		return colors;
	}

	public static Color[] Complementary(Color _Base)
	{
		Color[] colors = new Color[4];
		
		colors[0] = _Base;
		colors[1] = _Base.ShiftH(180).DevianceS(10);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[3] = _Base.DevianceS(50);
		
		return colors;
	}

	public static Color[] SplitComplementary(Color _Base)
	{
		Color[] colors = new Color[4];
		
		colors[0] = _Base;
		colors[1] = _Base.ShiftH(150);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[3] = _Base.ShiftH(210);
		
		return colors;
	}

	public static Color[] Triad(Color _Base)
	{
		Color[] colors = new Color[4];
		
		colors[0] = _Base;
		colors[1] = _Base.ShiftH(120).DevianceS(10);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[3] = _Base.ShiftH(240).DevianceS(10);
		
		return colors;
	}

	public static Color[] Square(Color _Base)
	{
		Color[] colors = new Color[4];
		
		colors[0] = _Base;
		colors[1] = _Base.ShiftH(90).DevianceS(10);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[3] = _Base.ShiftH(270).DevianceS(10);
		
		return colors;
	}

	public static Color[] Analogous(Color _Base)
	{
		float angle = Random.Range(0.0f, 45.0f);
		
		Color[] colors = new Color[4];
		
		colors[0] = _Base;
		colors[1] = _Base.ShiftH(angle).ShiftS(5);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[3] = _Base.ShiftH(-angle).ShiftS(5);
		
		return colors;
	}

	public static Color[] Shades(Color _Base)
	{
		Color[] colors = new Color[4];
		
		colors[0] = _Base;
		colors[3] = _Base.ShiftV(-50);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[1] = _Base.ShiftV(-25);
		
		return colors;
	}

	public static Color[] GoldenRatio(Color _Base)
	{
		const float angle = 137.5f;
		
		Color[] colors = new Color[4];
		
		colors[0] = _Base;
		colors[1] = _Base.ShiftH(angle * 1);
		colors[2] = new Color(1, 1, 1, 0.75f);
		colors[3] = _Base.ShiftH(angle * 2);
		
		return colors;
	}

	static float GetH(this Color _Color)
	{
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		return h;
	}

	static float GetS(this Color _Color)
	{
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
		return s;
	}

	static float GetV(this Color _Color)
	{
		Color.RGBToHSV(_Color, out float h, out float s, out float v);
		
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
}