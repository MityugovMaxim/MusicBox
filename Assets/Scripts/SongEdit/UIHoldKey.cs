using System;
using System.Linq;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

[ExecuteInEditMode]
public class UIHoldKey : UIEntity, IPointerClickHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
{
	[Preserve]
	public class Pool : MonoMemoryPool<RectTransform, Vector2, float, UIHoldKey>
	{
		protected override void Reinitialize(RectTransform _Container, Vector2 _Position, float _Size, UIHoldKey _Item)
		{
			Vector2 pivot = _Container.pivot;
			_Item.RectTransform.SetParent(_Container, false);
			_Item.RectTransform.anchorMin        = pivot;
			_Item.RectTransform.anchorMax        = pivot;
			_Item.RectTransform.pivot            = new Vector2(0.5f, 0.5f);
			_Item.RectTransform.anchoredPosition = _Position;
			_Item.RectTransform.sizeDelta        = Vector2.one * _Size;
			_Item.Container                      = _Container;
		}
	}

	RectTransform Container { get; set; }

	float Padding => Container.rect.width / 8;

	[SerializeField] GameObject m_Highlight;

	[Inject] UIBeat             m_Beat;
	[Inject] UIPlayer           m_Player;
	[Inject] UICreateHoldHandle m_CreateHoldHandle;

	ASFHoldKey  m_Key;
	ASFHoldClip m_Clip;
	Rect        m_ClipRect;
	Action      m_Reposition;
	Action      m_Rebuild;
	bool        m_Drag;

	readonly ClickCounter m_RemoveKey = new ClickCounter(2);

	public void Setup(ASFHoldKey _Key, ASFHoldClip _Clip, Rect _ClipRect, Action _Reposition, Action _Rebuild)
	{
		m_Key        = _Key;
		m_Clip       = _Clip;
		m_ClipRect   = _ClipRect;
		m_Reposition = _Reposition;
		m_Rebuild    = _Rebuild;
	}

	public void Process()
	{
		double time = m_Clip.MinTime + m_Key.Time;
		
		m_Highlight.SetActive(m_Player.Time >= time);
	}

	void Reposition(Vector2 _Offset, bool _Snap)
	{
		if (m_Clip?.Keys == null || m_Clip.Keys.Count < 2 || m_Key == null)
			return;
		
		Rect rect = m_Beat.GetLocalRect()
			.HorizontalResize(m_ClipRect.width, m_Beat.RectTransform.pivot)
			.HorizontalPadding(Padding);
		
		Vector2 point = (RectTransform.anchoredPosition + _Offset)
			.TransformPoint(Container, m_Beat.RectTransform)
			.HorizontalClamp(rect.xMin, rect.xMax);
		
		double minTime = m_Beat.Time + m_Beat.MinTime;
		double maxTime = m_Beat.Time + m_Beat.MaxTime;
		
		double time     = ASFMath.PositionToTime(point.y, rect.yMin, rect.yMax, minTime, maxTime);
		float  position = ASFMath.PositionToPhase(point.x, rect.xMin, rect.xMax);
		
		if (_Snap)
		{
			time = m_Beat.Snap(time);
			position = m_Clip.Keys.FirstOrDefault() == m_Key || m_Clip.Keys.LastOrDefault() == m_Key
				? ASFMath.SnapPhase(position, 1f / 3f)
				: ASFMath.SnapPhase(position, 1f / 6f);
		}
		
		m_Key.Time     = ASFMath.Remap(time, m_Clip.MinTime, m_Clip.MaxTime, 0, m_Clip.Length);
		m_Key.Position = position;
		
		point = new Vector2(
			ASFMath.PhaseToPosition(position, rect.xMin, rect.xMax),
			ASFMath.TimeToPosition(time, minTime, maxTime, rect.yMin, rect.yMax)
		);
		
		RectTransform.anchoredPosition = point.TransformPoint(m_Beat.RectTransform, Container);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		if (m_Drag)
		{
			m_Drag = false;
			return;
		}
		
		_EventData.Use();
		
		if (m_Clip.Keys.Count <= 2 || !m_RemoveKey.Execute(_EventData))
			return;
		
		m_Clip.Keys.Remove(m_Key);
		m_Rebuild?.Invoke();
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		m_Drag = true;
		
		_EventData.Use();
		
		Vector2 delta = RectTransform.InverseTransformVector(_EventData.delta);
		
		Reposition(delta, false);
		
		m_Reposition?.Invoke();
	}

	void IEndDragHandler.OnEndDrag(PointerEventData _EventData)
	{
		_EventData.Use();
		
		Reposition(Vector2.zero, true);
		
		m_Rebuild?.Invoke();
		
		m_Player.SortTrack<ASFHoldTrack, ASFHoldClip>();
	}

	bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 _Position, Camera _Camera)
	{
		return m_CreateHoldHandle != null && m_CreateHoldHandle.gameObject.activeSelf;
	}
}