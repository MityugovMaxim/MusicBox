﻿using UnityEngine;
using System.Collections.Generic;
using AudioBox.ASF;
using Zenject;

public class UITapTrack : ASFTrackContext<ASFTapClip>
{
	[Inject] UITapIndicator.Pool m_ItemPool;
	[Inject] UIInputReceiver     m_InputReceiver;

	readonly Dictionary<ASFTapClip, UITapIndicator> m_Items = new Dictionary<ASFTapClip, UITapIndicator>();
	readonly HashSet<ASFClip>                       m_Used  = new HashSet<ASFClip>();

	public override void AddClip(ASFTapClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		if (m_Used.Contains(_Clip))
			return;
		
		UITapIndicator item = m_ItemPool.Spawn(RectTransform, _Clip, UseClip);
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
		
		m_Items[_Clip] = item;
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(item);
	}

	public override void RemoveClip(ASFTapClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		if (!m_Items.ContainsKey(_Clip))
			return;
		
		UITapIndicator item = m_Items[_Clip];
		
		m_Items.Remove(_Clip);
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
		
		m_ItemPool.Despawn(item);
		
		if (m_InputReceiver != null)
			m_InputReceiver.UnregisterIndicator(item);
	}

	public override void ProcessClip(ASFTapClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		if (!m_Items.ContainsKey(_Clip))
			return;
		
		UITapIndicator item = m_Items[_Clip];
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
	}

	public override void Clear()
	{
		foreach (UITapIndicator item in m_Items.Values)
		{
			if (item == null)
				continue;
			
			m_ItemPool.Despawn(item);
			
			if (m_InputReceiver != null)
				m_InputReceiver.UnregisterIndicator(item);
		}
		
		m_Items.Clear();
		m_Used.Clear();
	}

	void UseClip(ASFClip _Clip)
	{
		m_Used.Add(_Clip);
	}
}
