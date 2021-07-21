using UnityEngine;
using System.Collections.Generic;

public class UITapTrack : UITrack<TapClip>
{
	const int MIN_CAPACITY = 5;

	Pool<UITapIndicator> Pool { get; set; }

	[SerializeField] UITapIndicator  m_Indicator;
	[SerializeField] UIInputReceiver m_InputReceiver;

	readonly Dictionary<TapClip, UITapIndicator> m_Indicators = new Dictionary<TapClip, UITapIndicator>();

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (Pool != null)
			Pool.Release();
	}

	public override void Initialize(List<TapClip> _Clips)
	{
		base.Initialize(_Clips);
		
		if (Pool == null)
			Pool = new Pool<UITapIndicator>(m_Indicator, MIN_CAPACITY);
	}

	protected override void RemoveIndicator(TapClip _Clip)
	{
		if (!m_Indicators.ContainsKey(_Clip))
			return;
		
		UITapIndicator indicator = m_Indicators[_Clip];
		
		indicator.Restore();
		
		m_Indicators.Remove(_Clip);
		m_InputReceiver.UnregisterIndicator(indicator);
		
		Pool.Remove(indicator);
	}

	protected override void DrawIndicators(List<TapClip> _Clips)
	{
		Vector2 anchor = new Vector2(
			0.5f,
			RectTransform.GetVerticalAnchor(m_InputReceiver.Zone, 0.5f)
		);
		
		foreach (TapClip clip in _Clips)
		{
			if (!m_Indicators.ContainsKey(clip))
				m_Indicators[clip] = CreateIndicator();
			
			UITapIndicator indicator = m_Indicators[clip];
			
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

	UITapIndicator CreateIndicator()
	{
		UITapIndicator indicator = Pool.Instantiate(m_Indicator, RectTransform);
		
		indicator.Setup();
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(indicator);
		
		return indicator;
	}
}
