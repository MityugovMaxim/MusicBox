using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIDoubleTrack : UITrack<DoubleClip>
{
	UIInputReceiver        m_InputReceiver;
	UIDoubleIndicator.Pool m_IndicatorPool;

	readonly HashSet<DoubleClip>                       m_Used       = new HashSet<DoubleClip>();
	readonly Dictionary<DoubleClip, UIDoubleIndicator> m_Indicators = new Dictionary<DoubleClip, UIDoubleIndicator>();

	[Inject]
	public void Construct(UIInputReceiver _InputReceiver, UIDoubleIndicator.Pool _IndicatorPool)
	{
		m_InputReceiver = _InputReceiver;
		m_IndicatorPool = _IndicatorPool;
	}

	public override void Initialize(float _Speed, List<DoubleClip> _Clips)
	{
		base.Initialize(_Speed, _Clips);
		
		m_Used.Clear();
	}

	protected override void RemoveIndicator(DoubleClip _Clip)
	{
		if (!m_Indicators.ContainsKey(_Clip))
			return;
		
		UIDoubleIndicator indicator = m_Indicators[_Clip];
		
		indicator.Restore();
		
		m_Indicators.Remove(_Clip);
		m_InputReceiver.UnregisterIndicator(indicator);
		
		if (Application.isPlaying)
			m_IndicatorPool.Despawn(indicator);
		else
			DestroyImmediate(indicator.gameObject);
	}

	protected override void DrawIndicators(List<DoubleClip> _Clips)
	{
		Vector2 anchor = GetAnchor();
		
		foreach (DoubleClip clip in _Clips)
		{
			if (!m_Indicators.ContainsKey(clip) && !m_Used.Contains(clip))
				m_Indicators[clip] = CreateIndicator(clip);
			
			UIDoubleIndicator indicator = m_Indicators[clip];
			
			if (indicator == null)
				continue;
			
			indicator.RectTransform.anchorMin        = anchor;
			indicator.RectTransform.anchorMax        = anchor;
			indicator.RectTransform.anchoredPosition = new Vector2(0, GetDistance(clip.MinTime));
		}
	}

	UIDoubleIndicator CreateIndicator(DoubleClip _Clip)
	{
		UIDoubleIndicator indicator = m_IndicatorPool.Spawn();
		
		if (indicator == null)
			return null;
		
		indicator.RectTransform.SetParent(RectTransform, false);
		
		indicator.Setup(() => m_Used.Add(_Clip));
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(indicator);
		
		return indicator;
	}
}