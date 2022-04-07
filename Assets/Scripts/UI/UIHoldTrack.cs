using System.Collections.Generic;
using AudioBox.ASF;
using UnityEngine;
using Zenject;

public class UIHoldTrack : ASFTrackContext<ASFHoldClip>
{
	[Inject] UIHoldIndicator.Pool m_ItemPool;
	[Inject] UIInputReceiver      m_InputReceiver;

	readonly Dictionary<ASFHoldClip, UIHoldIndicator> m_Items = new Dictionary<ASFHoldClip, UIHoldIndicator>();
	readonly HashSet<ASFClip>                         m_Used  = new HashSet<ASFClip>();

	public override void AddClip(ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		if (m_Used.Contains(_Clip))
			return;
		
		UIHoldIndicator item = m_ItemPool.Spawn(RectTransform, _Clip, UseClip);
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
		
		item.Build(_Clip);
		item.Process(_Clip.Phase);
		
		m_Items[_Clip] = item;
		
		if (m_InputReceiver != null)
			m_InputReceiver.RegisterIndicator(item);
	}

	public override void RemoveClip(ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		if (!m_Items.ContainsKey(_Clip))
			return;
		
		UIHoldIndicator item = m_Items[_Clip];
		
		m_Items.Remove(_Clip);
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
		
		item.Process(_Clip.Phase);
		
		m_ItemPool.Despawn(item);
		
		if (m_InputReceiver != null)
			m_InputReceiver.UnregisterIndicator(item);
	}

	public override void ProcessClip(ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		if (!m_Items.ContainsKey(_Clip))
			return;
		
		UIHoldIndicator item = m_Items[_Clip];
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
		
		item.Process(_Clip.Phase);
	}

	public override void Clear()
	{
		foreach (UIHoldIndicator item in m_Items.Values)
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
