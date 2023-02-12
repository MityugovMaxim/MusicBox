using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class UIUnitLabel : UIEntity
{
	public enum UnitType
	{
		None         = 0,
		Percent      = 1,
		Multiplier   = 2,
		Points       = 3,
		Coins        = 4,
		Date         = 5,
		Seconds      = 6,
		Minutes      = 7,
		Hours        = 8,
		Days         = 9,
		Badge        = 10,
		Milliseconds = 11,
		Season       = 12,
	}

	public enum UnitPosition
	{
		Right = 0,
		Left  = 1,
	}

	public double Value
	{
		get => m_Value;
		set
		{
			if (MathUtility.Approximately(m_Value, value))
				return;
			
			m_Value = value;
			
			ProcessValue();
		}
	}

	static readonly NumberFormatInfo m_FormatInfo = new NumberFormatInfo()
	{
		NumberGroupSizes       = new int[] { 3 },
		NumberDecimalDigits    = 0,
		NumberGroupSeparator   = " ",
		NumberDecimalSeparator = ".",
		PositiveSign           = "+",
		NegativeSign           = "-",
	};

	[SerializeField] UnitType     m_Type;
	[SerializeField] UnitPosition m_Position;
	[SerializeField] TMP_Text     m_Label;
	[SerializeField] double       m_Value;
	[SerializeField] bool         m_Sign;
	[SerializeField] bool         m_Short;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying || m_Label == null)
			return;
		
		ProcessValue();
	}
	#endif

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessValue();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessValue();
	}

	void ProcessValue()
	{
		m_Label.text = Convert(GetText(Value, m_Type));
	}

	string GetText(double _Value, UnitType _Type)
	{
		string sign = m_Sign && _Value > 0 ? "+" : string.Empty;
		
		if (m_Short)
		{
			
		}
		
		switch (_Type)
		{
			case UnitType.None:         return string.Format(m_FormatInfo, "{0}{1:0.##}", sign, _Value);
			case UnitType.Percent:      return string.Format(m_FormatInfo, GetMask("{0}{1:0.##}", '%'), sign, _Value);
			case UnitType.Multiplier:   return string.Format(m_FormatInfo, GetMask("{0}{1:#,##0.##}", '*'), sign, _Value);
			case UnitType.Points:       return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'p'), sign, Math.Truncate(_Value));
			case UnitType.Coins:        return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'c'), sign, Math.Truncate(_Value));
			case UnitType.Season:       return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'x'), sign, Math.Truncate(_Value));
			case UnitType.Seconds:      return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 's'), sign, Math.Truncate(_Value));
			case UnitType.Minutes:      return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'm'), sign, Math.Truncate(_Value));
			case UnitType.Hours:        return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'h'), sign, Math.Truncate(_Value));
			case UnitType.Days:         return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'd'), sign, Math.Truncate(_Value));
			case UnitType.Milliseconds: return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 't'), sign, Math.Truncate(_Value * 1000));
			case UnitType.Badge:
				int count = Mathf.Max(0, (int)_Value);
				return count > 9 ? "9+" : count.ToString();
			case UnitType.Date:
				DateTime date = TimeUtility.GetLocalTime(_Value);
				return date.Day == DateTime.Today.Day
					? date.ToShortTimeString()
					: date.ToShortDateString();
			default: return _Value.ToString(CultureInfo.InvariantCulture);
		}
	}

	string GetMask(string _Mask, char _Unit)
	{
		switch (m_Position)
		{
			case UnitPosition.Right:
				return _Mask + _Unit;
			case UnitPosition.Left:
				return _Unit + _Mask;
			default:
				return _Mask;
		}
	}

	static string Convert(string _Text)
	{
		StringBuilder text = new StringBuilder();
		foreach (char symbol in _Text)
			text.Append(Convert(symbol));
		return text.ToString();
	}

	static string Convert(char _Symbol)
	{
		switch (_Symbol)
		{
			case '%': return "<sup>%</sup>";
			case '*': return "<sup>X</sup>";
			case 'p': return "<sup>PTS</sup>";
			case 's': return "<sup>SEC</sup>";
			case 'm': return "<sup>MIN</sup>";
			case 'h': return "<sup>HRS</sup>";
			case 'd': return "<sup>DAYS</sup>";
			case 'c': return "<sprite tint=1 name=coins>";
			case 'x': return "<sprite tint=1 name=points>";
			case 't': return "<sup>MS</sup>";
			default:  return _Symbol.ToString();
		}
	}

	public static implicit operator TMP_Text(UIUnitLabel _Label)
	{
		return _Label.m_Label;
	}
}
