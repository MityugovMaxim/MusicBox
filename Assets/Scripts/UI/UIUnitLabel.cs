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
		None       = 0,
		Percent    = 1,
		Multiplier = 2,
		Points     = 3,
		Coins      = 4,
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
			if (m_Value.Equals(value))
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
	[SerializeField] bool         m_Tint;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
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
		m_Label.text = Convert(GetText(Value, m_Type), m_Tint);
	}

	string GetText(double _Value, UnitType _Type)
	{
		string sign = m_Sign && _Value > 0 ? "+" : string.Empty;
		switch (_Type)
		{
			case UnitType.Percent:    return string.Format(m_FormatInfo, GetMask("{0}{1:0.##}", '%'), sign, _Value);
			case UnitType.Multiplier: return string.Format(m_FormatInfo, GetMask("{0}{1:#,##0.##}", '*'), sign, _Value);
			case UnitType.Points:     return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'p'), sign, Math.Truncate(_Value));
			case UnitType.Coins:      return string.Format(m_FormatInfo, GetMask("{0}{1:N}", 'c'), sign, Math.Truncate(_Value));
			default:                  return string.Format(m_FormatInfo, "{0}{1:#,##0.##}", sign, _Value);
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

	static string Convert(string _Text, bool _Tint)
	{
		StringBuilder text = new StringBuilder();
		foreach (char symbol in _Text)
			text.Append(Convert(symbol, _Tint));
		return text.ToString();
	}

	static string Convert(char _Symbol, bool _Tint)
	{
		switch (_Symbol)
		{
			case '0': return "<sprite tint=1 name=unit_font_0>";
			case '1': return "<sprite tint=1 name=unit_font_1>";
			case '2': return "<sprite tint=1 name=unit_font_2>";
			case '3': return "<sprite tint=1 name=unit_font_3>";
			case '4': return "<sprite tint=1 name=unit_font_4>";
			case '5': return "<sprite tint=1 name=unit_font_5>";
			case '6': return "<sprite tint=1 name=unit_font_6>";
			case '7': return "<sprite tint=1 name=unit_font_7>";
			case '8': return "<sprite tint=1 name=unit_font_8>";
			case '9': return "<sprite tint=1 name=unit_font_9>";
			case '.': return "<sprite tint=1 name=unit_font_dot>";
			case '+': return "<sprite tint=1 name=unit_font_plus>";
			case '-': return "<sprite tint=1 name=unit_font_minus>";
			case '%': return "<sprite tint=1 name=unit_font_percent>";
			case '*': return "<sprite tint=1 name=unit_font_multiplier>";
			case 'p': return "<sprite tint=1 name=unit_font_points>";
			case 'c': return $"<sprite tint={(_Tint ? 1 : 0)} name=coins_icon>";
			default:  return _Symbol.ToString();
		}
	}

	public static implicit operator TMP_Text(UIUnitLabel _Label)
	{
		return _Label.m_Label;
	}
}