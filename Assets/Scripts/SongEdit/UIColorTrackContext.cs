using System.Collections.Generic;
using AudioBox.ASF;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UIColorTrackContext : ASFTrackContext<ASFColorClip>, IASFColorSampler
{
	static readonly int m_BackgroundPrimaryPropertyID   = Shader.PropertyToID("_BackgroundPrimaryColor");
	static readonly int m_BackgroundSecondaryPropertyID = Shader.PropertyToID("_BackgroundSecondaryColor");
	static readonly int m_ForegroundPrimaryPropertyID   = Shader.PropertyToID("_ForegroundPrimaryColor");
	static readonly int m_ForegroundSecondaryPropertyID = Shader.PropertyToID("_ForegroundSecondaryColor");

	[Inject] UIColorClipContext.Pool m_ItemPool;

	readonly Dictionary<ASFColorClip, UIColorClipContext> m_Views = new Dictionary<ASFColorClip, UIColorClipContext>();

	public override void AddClip(ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UIColorClipContext item = m_ItemPool.Spawn(RectTransform, _Clip, _ClipRect, _ViewRect);
		
		AddView(_Clip, item);
	}

	public override void RemoveClip(ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UIColorClipContext item = GetView(_Clip);
		
		m_ItemPool.Despawn(item);
		
		RemoveView(_Clip);
	}

	public override void ProcessClip(ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		UIColorClipContext item = GetView(_Clip);
		
		if (item == null)
			return;
		
		item.ClipRect = _ClipRect;
		item.ViewRect = _ViewRect;
	}

	public override void Clear()
	{
		foreach (UIColorClipContext context in m_Views.Values)
			m_ItemPool.Despawn(context);
		m_Views.Clear();
	}

	void AddView(ASFColorClip _Clip, UIColorClipContext _View)
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

	void RemoveView(ASFColorClip _Clip)
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

	UIColorClipContext GetView(ASFColorClip _Clip)
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

	void IASFColorSampler.Sample(ASFColorClip _Source, ASFColorClip _Target, float _Phase)
	{
		SetColor(m_BackgroundPrimaryPropertyID, _Source.BackgroundPrimary, _Target.BackgroundPrimary, _Phase);
		SetColor(m_BackgroundSecondaryPropertyID, _Source.BackgroundSecondary, _Target.BackgroundSecondary, _Phase);
		SetColor(m_ForegroundPrimaryPropertyID, _Source.ForegroundPrimary, _Target.ForegroundPrimary, _Phase);
		SetColor(m_ForegroundSecondaryPropertyID, _Source.ForegroundSecondary, _Target.ForegroundSecondary, _Phase);
	}

	static void SetColor(int _ColorPropertyID, Color _Source, Color _Target, float _Phase)
	{
		Color color = Color.Lerp(_Source, _Target, _Phase);
		
		Shader.SetGlobalColor(_ColorPropertyID, color);
	}
}