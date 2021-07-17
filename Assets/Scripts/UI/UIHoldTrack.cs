using System.Collections.Generic;
using UnityEngine;

public class UIHoldTrack : UITrack<HoldClip>
{
	const int MIN_CAPACITY = 2;

	Pool<UIHoldIndicator> Pool { get; set; }

	[SerializeField] UIHoldIndicator m_Indicator;
	[SerializeField] UIInputReceiver m_InputReceiver;

	readonly Dictionary<HoldClip, UIHoldIndicator> m_Indicators = new Dictionary<HoldClip, UIHoldIndicator>();

	public override void Initialize(List<HoldClip> _Clips)
	{
		base.Initialize(_Clips);
		
		if (Pool != null)
			Pool.Release();
		else
			Pool = new Pool<UIHoldIndicator>(m_Indicator, MIN_CAPACITY);
		
		#if UNITY_EDITOR
		UIHoldIndicator[] indicators = GetComponentsInChildren<UIHoldIndicator>();
		foreach (UIHoldIndicator indicator in indicators)
			Pool.Remove(indicator);
		#endif
	}

	protected override void RemoveIndicator(HoldClip _Clip)
	{
		if (_Clip == null || !m_Indicators.ContainsKey(_Clip))
			return;
		
		UIHoldIndicator indicator = m_Indicators[_Clip];
		
		if (indicator == null)
			return;
		
		indicator.Restore();
		
		m_Indicators.Remove(_Clip);
		m_InputReceiver.UnregisterIndicator(indicator);
		
		Pool.Remove(indicator);
	}

	protected override void DrawIndicators(List<HoldClip> _Clips)
	{
		Vector2 anchor = new Vector2(
			0.5f,
			RectTransform.GetVerticalAnchor(m_InputReceiver.Zone, 0.5f)
		);
		
		foreach (HoldClip clip in _Clips)
		{
			if (!m_Indicators.ContainsKey(clip))
				m_Indicators[clip] = CreateIndicator(clip);
			
			UIHoldIndicator indicator = m_Indicators[clip];
			
			if (indicator == null)
				continue;
			
			indicator.RectTransform.anchorMin        = anchor;
			indicator.RectTransform.anchorMax        = anchor;
			indicator.RectTransform.anchoredPosition = new Vector2(0, GetDistance(clip.MinTime));
			
			float progress = Mathf.InverseLerp(clip.MinTime, clip.MaxTime, Time);
			
			indicator.Process(progress);
		}
		
		m_InputReceiver.Process();
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

	UIHoldIndicator CreateIndicator(HoldClip _Clip)
	{
		UIHoldIndicator indicator = Pool.Instantiate(m_Indicator, RectTransform);
		
		if (indicator == null)
			return null;
		
		float duration = GetDistance(_Clip.MaxTime) - GetDistance(_Clip.MinTime);
		
		indicator.Setup(_Clip, duration);
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(indicator);
		
		return indicator;
	}
}
