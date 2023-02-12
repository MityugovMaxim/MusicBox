using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UIDoubleClipContext : ASFClipContext<ASFDoubleClip>, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
{
	[Preserve]
	public class Pool : MonoMemoryPool<RectTransform, ASFDoubleClip, Rect, Rect, UIDoubleClipContext>
	{
		protected override void Reinitialize(RectTransform _Container, ASFDoubleClip _Clip, Rect _ClipRect, Rect _ViewRect, UIDoubleClipContext _Item)
		{
			_Item.Setup(_Container, _Clip, _ClipRect, _ViewRect);
			
			_Item.Select(ClipSelection.Contains(_Item.Clip));
		}

		protected override void OnSpawned(UIDoubleClipContext _Item)
		{
			base.OnSpawned(_Item);
			
			ClipSelection.Changed += _Item.OnSelectionChanged;
		}

		protected override void OnDespawned(UIDoubleClipContext _Item)
		{
			base.OnDespawned(_Item);
			
			ClipSelection.Changed -= _Item.OnSelectionChanged;
		}
	}

	[SerializeField] GameObject m_Selection;
	[SerializeField] GameObject m_Highlight;

	[Inject] UIBeat               m_Beat;
	[Inject] UIPlayer             m_Player;
	[Inject] UICreateDoubleHandle m_CreateDoubleHandle;

	public void Process()
	{
		m_Highlight.SetActive(m_Player.Time >= Clip.Time);
	}

	void Select(bool _Value)
	{
		m_Selection.SetActive(_Value);
	}

	void OnSelectionChanged()
	{
		Select(ClipSelection.Contains(Clip));
	}

	void Reposition(Vector2 _Offset, bool _Snap)
	{
		Rect rect = m_Beat.GetLocalRect().HorizontalResize(ClipRect.width, m_Beat.RectTransform.pivot);
		
		Vector2 point = (RectTransform.anchoredPosition + _Offset).TransformPoint(Container, m_Beat.RectTransform);
		
		double minTime = m_Beat.Time + m_Beat.MinTime;
		double maxTime = m_Beat.Time + m_Beat.MaxTime;
		
		double time = ASFMath.PositionToTime(point.y, rect.yMin, rect.yMax, minTime, maxTime);
		
		if (_Snap)
			time = m_Beat.Snap(time);
		
		Clip.Time = time;
		
		point = new Vector2(point.x, ASFMath.TimeToPosition(time, minTime, maxTime, rect.yMin, rect.yMax));
		
		RectTransform.anchoredPosition = point.TransformPoint(m_Beat.RectTransform, Container);
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		_EventData.Use();
		
		Vector2 delta = RectTransform.InverseTransformVector(_EventData.delta);
		
		Reposition(new Vector2(0, delta.y), false);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData _EventData)
	{
		_EventData.Use();
		
		Reposition(Vector2.zero, true);
		
		m_Player.SortTrack<ASFDoubleTrack, ASFDoubleClip>();
	}

	bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 _Position, Camera _Camera)
	{
		return m_CreateDoubleHandle != null && m_CreateDoubleHandle.gameObject.activeSelf;
	}
}
