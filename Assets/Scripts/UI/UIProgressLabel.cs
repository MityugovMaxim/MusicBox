using System;
using TMPro;
using UnityEngine;

public class UIProgressLabel : UIEntity
{
	public long Progress
	{
		get => m_Progress;
		set
		{
			if (m_Progress == value)
				return;
			
			m_Progress = value;
			
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

	const string PROGRESS_ICON = "progress_icon";

	[SerializeField] TMP_Text m_Label;
	[SerializeField] bool     m_Sign;
	[SerializeField] long     m_Progress;
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
		string sign       = m_Sign ? Progress >= 0 ? "+" : "-" : string.Empty;
		string multiplier = m_Multiplier >= 2 ? $" ×{Multiplier}" : string.Empty;
		
		m_Label.text = $"{sign}{Math.Abs(Progress)}<sprite name=\"{PROGRESS_ICON}\">{multiplier}";
	}
}