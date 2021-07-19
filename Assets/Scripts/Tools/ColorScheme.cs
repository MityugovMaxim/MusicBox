using System;
using UnityEngine;

[Serializable]
public struct ColorScheme
{
	public Color BackgroundPrimary   => m_BackgroundPrimary;
	public Color BackgroundSecondary => m_BackgroundSecondary;
	public Color ForegroundPrimary   => m_ForegroundPrimary;
	public Color ForegroundSecondary => m_ForegroundSecondary;

	[SerializeField] Color m_BackgroundPrimary;
	[SerializeField] Color m_BackgroundSecondary;
	[SerializeField] Color m_ForegroundPrimary;
	[SerializeField] Color m_ForegroundSecondary;

	public ColorScheme(Color _BackgroundPrimary, Color _BackgroundSecondary, Color _ForegroundPrimary, Color _ForegroundSecondary)
	{
		m_BackgroundPrimary   = _BackgroundPrimary;
		m_BackgroundSecondary = _BackgroundSecondary;
		m_ForegroundPrimary   = _ForegroundPrimary;
		m_ForegroundSecondary = _ForegroundSecondary;
	}

	public bool Equals(ColorScheme _ColorScheme)
	{
		return m_BackgroundPrimary.Equals(_ColorScheme.m_BackgroundPrimary) && m_BackgroundSecondary.Equals(_ColorScheme.m_BackgroundSecondary) && m_ForegroundPrimary.Equals(_ColorScheme.m_ForegroundPrimary) && m_ForegroundSecondary.Equals(_ColorScheme.m_ForegroundSecondary);
	}

	public override bool Equals(object _Object)
	{
		return _Object is ColorScheme other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = BackgroundPrimary.GetHashCode();
			hashCode = (hashCode * 397) ^ BackgroundSecondary.GetHashCode();
			hashCode = (hashCode * 397) ^ ForegroundPrimary.GetHashCode();
			hashCode = (hashCode * 397) ^ ForegroundSecondary.GetHashCode();
			return hashCode;
		}
	}

	public static bool operator ==(ColorScheme _Left, ColorScheme _Right)
	{
		return _Left.Equals(_Right);
	}

	public static bool operator !=(ColorScheme _Left, ColorScheme _Right)
	{
		return !_Left.Equals(_Right);
	}

	public static ColorScheme Lerp(ColorScheme _Source, ColorScheme _Target, float _Phase)
	{
		return new ColorScheme(
			Color.Lerp(_Source.BackgroundPrimary, _Target.BackgroundPrimary, _Phase),
			Color.Lerp(_Source.BackgroundSecondary, _Target.BackgroundSecondary, _Phase),
			Color.Lerp(_Source.ForegroundPrimary, _Target.ForegroundPrimary, _Phase),
			Color.Lerp(_Source.ForegroundSecondary, _Target.ForegroundSecondary, _Phase)
		);
	}
}