using System.Collections.Generic;
using UnityEngine;

public class UIDoubleTrack : UITrack<DoubleClip>
{
	const int MIN_CAPACITY = 5;

	Pool<UIDoubleIndicator> Pool { get; set; }

	[SerializeField] UIDoubleIndicator m_Indicator;
	[SerializeField] UIInputReceiver   m_InputReceiver;

	readonly Dictionary<DoubleClip, UIDoubleIndicator> m_Indicators = new Dictionary<DoubleClip, UIDoubleIndicator>();

	public override void Initialize(List<DoubleClip> _Clips)
	{
		base.Initialize(_Clips);
		
		if (Pool != null)
			Pool.Release();
		else
			Pool = new Pool<UIDoubleIndicator>(m_Indicator, MIN_CAPACITY);
		
		#if UNITY_EDITOR
		UIDoubleIndicator[] indicators = GetComponentsInChildren<UIDoubleIndicator>();
		foreach (UIDoubleIndicator indicator in indicators)
			Pool.Remove(indicator);
		#endif
	}

	protected override void RemoveIndicator(DoubleClip _Clip)
	{
		if (!m_Indicators.ContainsKey(_Clip))
			return;
		
		UIDoubleIndicator indicator = m_Indicators[_Clip];
		
		indicator.Restore();
		
		m_Indicators.Remove(_Clip);
		m_InputReceiver.UnregisterIndicator(indicator);
		
		Pool.Remove(indicator);
	}

	protected override void DrawIndicators(List<DoubleClip> _Clips)
	{
		Vector2 anchor = new Vector2(
			0.5f,
			RectTransform.GetVerticalAnchor(m_InputReceiver.Zone, 0.5f)
		);
		
		foreach (DoubleClip clip in _Clips)
		{
			if (!m_Indicators.ContainsKey(clip))
				m_Indicators[clip] = CreateIndicator();
			
			UIDoubleIndicator indicator = m_Indicators[clip];
			
			if (indicator == null)
				continue;
			
			indicator.RectTransform.anchorMin        = anchor;
			indicator.RectTransform.anchorMax        = anchor;
			indicator.RectTransform.anchoredPosition = new Vector2(0, GetDistance(clip.MinTime));
		}
	}

	protected override float GetMinTime()
	{
		Rect rect = RectTransform.GetLocalRect(m_InputReceiver.Zone);
		
		return GetTime(rect.yMin + m_Indicator.MinPadding);
	}

	protected override float GetMaxTime()
	{
		Rect rect = RectTransform.GetLocalRect(m_InputReceiver.Zone);
		
		return GetTime(rect.yMax + m_Indicator.MaxPadding);
	}

	UIDoubleIndicator CreateIndicator()
	{
		UIDoubleIndicator indicator = Pool.Instantiate(m_Indicator, RectTransform);
		
		indicator.Setup();
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(indicator);
		
		return indicator;
	}
}