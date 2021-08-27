﻿using UnityEngine;
using UnityEngine.UI;

public class UIGameProgress : UIEntity, ISampleReceiver
{
	[SerializeField, Range(0, 1)] float m_Progress;
	[SerializeField, Range(0, 1)] float m_Min;
	[SerializeField, Range(0, 1)] float m_Max;
	[SerializeField]              Image m_Graphic;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced)
			return;
		
		Sample(m_Progress);
	}
	#endif

	public void Sample(float _Time, float _Length)
	{
		Sample(_Time / _Length);
	}

	void Sample(float _Progress)
	{
		float progress = Mathf.Clamp01(_Progress);
		
		m_Progress = progress;
		
		if (m_Graphic != null)
			m_Graphic.rectTransform.anchorMax = new Vector2(MathUtility.Remap(m_Progress, 0, 1, m_Min, m_Max), 1);
	}
}