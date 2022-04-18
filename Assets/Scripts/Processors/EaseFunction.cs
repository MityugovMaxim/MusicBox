using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class EaseFunction
{
	public static readonly EaseFunction Default        = new EaseFunction(0.25f, 0.1f, 0.25f, 1.0f);
	public static readonly EaseFunction Linear         = new EaseFunction(0.0f, 0.0f, 1.0f, 1.0f);
	public static readonly EaseFunction EaseIn         = new EaseFunction(0.42f, 0.0f, 1.0f, 1.0f);
	public static readonly EaseFunction EaseOut        = new EaseFunction(0.0f, 0.0f, 0.58f, 1.0f);
	public static readonly EaseFunction EaseInOut      = new EaseFunction(0.42f, 0.0f, 0.58f, 1.0f);
	public static readonly EaseFunction EaseInBack     = new EaseFunction(0.6f, -0.28f, 0.735f, 0.045f);
	public static readonly EaseFunction EaseOutBack    = new EaseFunction(0.175f, 0.885f, 0.32f, 1.275f);
	public static readonly EaseFunction EaseInQuad     = new EaseFunction(0.55f, 0.085f, 0.68f, 0.53f);
	public static readonly EaseFunction EaseOutQuad    = new EaseFunction(0.25f, 0.46f, 0.45f, 0.94f);
	public static readonly EaseFunction EaseInOutQuad  = new EaseFunction(0.455f, 0.03f, 0.515f, 0.955f);
	public static readonly EaseFunction EaseOutCubic   = new EaseFunction(0.215f, 0.61f, 0.355f, 1f);
	public static readonly EaseFunction EaseInCubic    = new EaseFunction(0.55f, 0.055f, 0.675f, 0.19f);
	public static readonly EaseFunction EaseInOutCubic = new EaseFunction(0.645f, 0.045f, 0.355f, 1f);
	public static readonly EaseFunction EaseOutExpo    = new EaseFunction(0.19f, 1f, 0.22f, 1f);
	public static readonly EaseFunction EaseInExpo     = new EaseFunction(0.95f, 0.05f, 0.795f, 0.035f);
	public static readonly EaseFunction EaseInOutBack  = new EaseFunction(0.68f, -0.55f, 0.265f, 1.55f);

	readonly float m_A;
	readonly float m_B;
	readonly float m_C;
	readonly float m_D;

	public EaseFunction(float _A, float _B, float _C, float _D)
	{
		m_A = _A;
		m_B = _B;
		m_C = _C;
		m_D = _D;
	}

	public float Get(float _Phase)
	{
		float a = 1.0f - 3.0f * m_C + 3.0f * m_A;
		float b = 3.0f * m_C - 6.0f * m_A;
		float c = 3.0f * m_A;
		
		float e = 1.0f - 3.0f * m_D + 3.0f * m_B;
		float f = 3.0f * m_D - 6.0f * m_B;
		float g = 3.0f * m_B;
		
		float t = _Phase;
		for (int i = 0; i < 5; i++)
		{
			float dt = a * (t * t * t) + b * (t * t) + c * t;
			
			float s = 3.0f * a * t * t + 2.0f * b * t + c;
			if (!Mathf.Approximately(s, 0))
				t = Mathf.Clamp01(t - (dt - _Phase) / s);
		}
		
		return e * (t * t * t) + f * (t * t) + g * t;
	}

	public float Get(float _Source, float _Target, float _Phase)
	{
		return Mathf.LerpUnclamped(_Source, _Target, Get(_Phase));
	}

	public double Get(double _Source, double _Target, float _Phase)
	{
		return _Source + (_Target - _Source) * Get(_Phase);
	}

	public Vector2 Get(Vector2 _Source, Vector2 _Target, float _Phase)
	{
		return Vector2.LerpUnclamped(_Source, _Target, Get(_Phase));
	}

	public Vector3 Get(Vector3 _Source, Vector3 _Target, float _Phase)
	{
		return Vector3.LerpUnclamped(_Source, _Target, Get(_Phase));
	}

	public Vector4 Get(Vector4 _Source, Vector4 _Target, float _Phase)
	{
		return Vector4.LerpUnclamped(_Source, _Target, Get(_Phase));
	}

	public Rect Get(Rect _Source, Rect _Target, float _Phase)
	{
		float phase = Get(_Phase);
		return new Rect(
			Vector2.LerpUnclamped(_Source.position, _Target.position, phase),
			Vector2.LerpUnclamped(_Source.size, _Target.size, phase)
		);
	}

	public Color Get(Color _Source, Color _Target, float _Phase)
	{
		return Color.LerpUnclamped(_Source, _Target, Get(_Phase));
	}

	public Quaternion Get(Quaternion _Source, Quaternion _Target, float _Phase)
	{
		return Quaternion.Lerp(_Source, _Target, Get(_Phase));
	}
}