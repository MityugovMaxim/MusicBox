using System.Collections.Generic;
using AudioBox.ASF;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UITapTrackContext : ASFTrackContext<ASFTapClip>
{
	[Inject] UITapClipContext.Pool m_ItemPool;

	readonly Dictionary<ASFTapClip, UITapClipContext> m_Views = new Dictionary<ASFTapClip, UITapClipContext>();

	public override void AddClip(ASFTapClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UITapClipContext item = m_ItemPool.Spawn(RectTransform, _Clip, _ClipRect, _ViewRect);
		
		item.Process();
		
		AddView(_Clip, item);
	}

	public override void RemoveClip(ASFTapClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UITapClipContext item = GetView(_Clip);
		
		item.Process();
		
		RemoveView(_Clip);
		
		m_ItemPool.Despawn(item);
	}

	public override void ProcessClip(ASFTapClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UITapClipContext item = GetView(_Clip);
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
		
		item.Process();
	}

	public override void Clear()
	{
		foreach (UITapClipContext context in m_Views.Values)
			m_ItemPool.Despawn(context);
		m_Views.Clear();
	}

	void AddView(ASFTapClip _Clip, UITapClipContext _View)
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

	void RemoveView(ASFTapClip _Clip)
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

	UITapClipContext GetView(ASFTapClip _Clip)
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