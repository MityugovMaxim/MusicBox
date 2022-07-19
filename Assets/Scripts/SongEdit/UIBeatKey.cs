using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIBeatKey : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<RectTransform, UIBeatKey>
	{
		protected override void Reinitialize(RectTransform _Container, UIBeatKey _Item)
		{
			Vector2 pivot = _Container.pivot;
			_Item.RectTransform.SetParent(_Container, false);
			_Item.RectTransform.anchorMin = new Vector2(0, pivot.y);
			_Item.RectTransform.anchorMax = new Vector2(1, pivot.y);
			_Item.RectTransform.pivot     = new Vector2(0.5f, 0.5f);
		}
	}

	public float Position
	{
		get => RectTransform.anchoredPosition.y;
		set
		{
			Vector2 position = RectTransform.anchoredPosition;
			position.y                     = value;
			RectTransform.anchoredPosition = position;
		}
	}

	public double Time
	{
		get => m_Time;
		set
		{
			if (Math.Abs(m_Time - value) < double.Epsilon * 2)
				return;
			
			m_Time = value;
			
			ProcessTime();
		}
	}

	[SerializeField] TMP_Text m_TimeLabel;

	double m_Time;

	protected override void Awake()
	{
		base.Awake();
		
		ProcessTime();
	}

	void ProcessTime()
	{
		int minutes      = (int)m_Time / 60;
		int seconds      = (int)m_Time % 60;
		int milliseconds = (int)(m_Time * 1000) % 1000;
		
		m_TimeLabel.text = $"{minutes:00}:{seconds:00}.{milliseconds:000}";
	}
}