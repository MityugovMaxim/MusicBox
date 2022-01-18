using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIHoldTrack : UITrack<HoldClip>
{
	readonly Dictionary<HoldClip, UIHoldIndicator> m_Indicators = new Dictionary<HoldClip, UIHoldIndicator>();

	UIInputReceiver      m_InputReceiver;
	UIHoldIndicator.Pool m_IndicatorPool;

	[Inject]
	public void Construct(UIInputReceiver _InputReceiver, UIHoldIndicator.Pool _IndicatorPool)
	{
		m_InputReceiver = _InputReceiver;
		m_IndicatorPool = _IndicatorPool;
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
		
		m_IndicatorPool.Despawn(indicator);
	}

	protected override void DrawIndicators(List<HoldClip> _Clips)
	{
		Vector2 anchor = GetAnchor();
		
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
	}

	UIHoldIndicator CreateIndicator(HoldClip _Clip)
	{
		UIHoldIndicator indicator = m_IndicatorPool.Spawn();
		
		if (indicator == null)
			return null;
		
		indicator.RectTransform.SetParent(RectTransform, false);
		
		float duration = GetDistance(_Clip.MaxTime) - GetDistance(_Clip.MinTime);
		
		indicator.Setup(_Clip, duration);
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(indicator);
		
		return indicator;
	}
}
