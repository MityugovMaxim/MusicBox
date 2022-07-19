using System.Collections.Generic;
using AudioBox.ASF;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UIHoldTrackContext : ASFTrackContext<ASFHoldClip>
{
	[Inject] UIHoldClipContext.Pool m_ItemPool;

	readonly Dictionary<ASFHoldClip, UIHoldClipContext> m_Views = new Dictionary<ASFHoldClip, UIHoldClipContext>();

	public override void AddClip(ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UIHoldClipContext item = m_ItemPool.Spawn(RectTransform, _Clip, _ClipRect, _ViewRect);
		
		AddView(_Clip, item);
	}

	public override void RemoveClip(ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UIHoldClipContext item = GetView(_Clip);
		
		m_ItemPool.Despawn(item);
		
		RemoveView(_Clip);
	}

	public override void ProcessClip(ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UIHoldClipContext item = GetView(_Clip);
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
	}

	public override void Clear()
	{
		foreach (UIHoldClipContext context in m_Views.Values)
			m_ItemPool.Despawn(context);
		m_Views.Clear();
	}

	void AddView(ASFHoldClip _Clip, UIHoldClipContext _View)
	{
		if (_Clip == null)
		{
			Log.Error(this, "Add view failed. Clip is null.");
			return;
		}
		
		if (_View == null)
		{
			Log.Error(this, "Add view failed. View is null for clip '{0}'.", _Clip);
			return;
		}
		
		if (m_Views.ContainsKey(_Clip))
		{
			Log.Error(this, "Add view failed. View for clip '{0}' already exists.", _Clip);
			return;
		}
		
		m_Views[_Clip] = _View;
	}

	void RemoveView(ASFHoldClip _Clip)
	{
		if (_Clip == null)
		{
			Log.Error(this, "Remove view failed. Clip is null.");
			return;
		}
		
		if (!m_Views.ContainsKey(_Clip))
		{
			Log.Error(this, "Remove view failed. View for clip '{0}' not found.", _Clip);
			return;
		}
		
		m_Views.Remove(_Clip);
	}

	UIHoldClipContext GetView(ASFHoldClip _Clip)
	{
		if (_Clip == null)
		{
			Log.Error(this, "Get view failed. Clip is null.");
			return null;
		}
		
		if (!m_Views.ContainsKey(_Clip))
		{
			Log.Error(this, "Get view failed. View for clip '{0}' not found.", _Clip);
			return null;
		}
		
		return m_Views[_Clip];
	}
}