using System;
using System.Linq;
using UnityEngine;

public class UISongFrame : UIEntity
{
	[Serializable]
	public class ColorData
	{
		public ColorMode ColorMode => m_ColorMode;
		public Material  Outline   => m_Outline;
		public Material  Glow      => m_Glow;

		[SerializeField] ColorMode m_ColorMode;
		[SerializeField] Material  m_Outline;
		[SerializeField] Material  m_Glow;
	}

	public enum ColorMode
	{
		Blue,
		Green,
		Grey,
		Red
	}

	public ColorMode Mode
	{
		get => m_Mode;
		set
		{
			if (m_Mode == value)
				return;
			
			m_Mode = value;
			
			ProcessMode();
		}
	}

	[SerializeField] ColorData[] m_Colors;
	[SerializeField] UIRounded   m_Outline;
	[SerializeField] UIRounded   m_Glow;
	[SerializeField] ColorMode   m_Mode;

	void ProcessMode()
	{
		ColorData colorData = m_Colors.FirstOrDefault(_Data => _Data.ColorMode == Mode);
		
		if (colorData == null)
			return;
		
		if (colorData.Outline != null)
			m_Outline.material = colorData.Outline;
		
		if (colorData.Glow != null)
			m_Glow.material = colorData.Glow;
	}
}