using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class UITapTrack : UITrack<TapClip>
{
	UITapIndicator.Pool m_IndicatorPool;
	UIInputReceiver     m_InputReceiver;

	readonly Dictionary<TapClip, UITapIndicator> m_Indicators = new Dictionary<TapClip, UITapIndicator>();

	[Inject]
	public void Construct(UIInputReceiver _InputReceiver, UITapIndicator.Pool _IndicatorPool)
	{
		m_InputReceiver = _InputReceiver;
		m_IndicatorPool = _IndicatorPool;
	}

	protected override void RemoveIndicator(TapClip _Clip)
	{
		if (!m_Indicators.ContainsKey(_Clip))
			return;
		
		UITapIndicator indicator = m_Indicators[_Clip];
		
		indicator.Restore();
		
		m_Indicators.Remove(_Clip);
		m_InputReceiver.UnregisterIndicator(indicator);
		
		if (Application.isPlaying)
			m_IndicatorPool.Despawn(indicator);
		else
			DestroyImmediate(indicator.gameObject);
	}

	protected override void DrawIndicators(List<TapClip> _Clips)
	{
		Vector2 anchor = GetAnchor();
		
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

	UITapIndicator CreateIndicator()
	{
		UITapIndicator indicator = m_IndicatorPool.Spawn();
		
		if (indicator == null)
			return null;
		
		indicator.RectTransform.SetParent(RectTransform, false);
		
		indicator.Setup();
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(indicator);
		
		return indicator;
	}
}
