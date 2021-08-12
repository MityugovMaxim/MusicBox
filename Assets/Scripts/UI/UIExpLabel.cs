using System;
using TMPro;
using UnityEngine;

public class UIExpLabel : UIEntity
{
	public long Exp
	{
		get => m_Exp;
		set
		{
			if (m_Exp == value)
				return;
			
			m_Exp = value;
			
			ProcessProgress();
		}
	}

	public int Multiplier
	{
		get => m_Multiplier;
		set
		{
			if (m_Multiplier == value)
				return;
			
			m_Multiplier = value;
			
			ProcessProgress();
		}
	}

	const string EXP_ICON = "exp_icon";

	[SerializeField] TMP_Text m_Label;
	[SerializeField] bool     m_Sign;
	[SerializeField] long     m_Exp;
	[SerializeField] int      m_Multiplier = 1;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessProgress();
	}
	#endif

	void ProcessProgress()
	{
		string sign       = m_Sign ? Exp >= 0 ? "+" : "-" : string.Empty;
		string multiplier = m_Multiplier >= 2 ? $" ×{Multiplier}" : string.Empty;
		
		m_Label.text = $"{sign}{Math.Abs(Exp)}<sprite name=\"{EXP_ICON}\">{multiplier}";
	}
}