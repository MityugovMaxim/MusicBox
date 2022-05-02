using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

[ExecuteInEditMode]
public class UITapClipContext : ASFClipContext<ASFTapClip>, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
{
	[Preserve]
	public class Pool : MonoMemoryPool<RectTransform, ASFTapClip, Rect, Rect, UITapClipContext>
	{
		protected override void Reinitialize(RectTransform _Container, ASFTapClip _Clip, Rect _ClipRect, Rect _ViewRect, UITapClipContext _Item)
		{
			_Item.Setup(_Container, _Clip, _ClipRect, _ViewRect);
			
			_Item.Select(ClipSelection.Contains(_Item.Clip));
		}

		protected override void OnSpawned(UITapClipContext _Item)
		{
			base.OnSpawned(_Item);
			
			ClipSelection.Changed += _Item.OnSelectionChange;
		}

		protected override void OnDespawned(UITapClipContext _Item)
		{
			base.OnDespawned(_Item);
			
			ClipSelection.Changed -= _Item.OnSelectionChange;
		}
	}

	float Padding => Container.rect.width / 8;

	[SerializeField] GameObject m_Selection;

	[Inject] UIBeat            m_Beat;
	[Inject] UIPlayer          m_Player;
	[Inject] UICreateTapHandle m_CreateTapHandle;

	void Select(bool _Value)
	{
		m_Selection.SetActive(_Value);
	}

	void OnSelectionChange()
	{
		Select(ClipSelection.Contains(Clip));
	}

	void Reposition(Vector2 _Offset, bool _Snap)
	{
		Rect rect = m_Beat.GetLocalRect()
			.HorizontalResize(ClipRect.width, m_Beat.RectTransform.pivot)
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
			time     = m_Beat.Snap(time);
			position = ASFMath.SnapPhase(position, 1f / 3f);
		}
		
		Clip.Time     = time;
		Clip.Position = position;
		
		point = new Vector2(
			ASFMath.PhaseToPosition(position, rect.xMin, rect.xMax),
			ASFMath.TimeToPosition(time, minTime, maxTime, rect.yMin, rect.yMax)
		);
		
		RectTransform.anchoredPosition = point.TransformPoint(m_Beat.RectTransform, Container);
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		_EventData.Use();
		
		Vector2 delta = RectTransform.InverseTransformVector(_EventData.delta);
		
		Reposition(delta, false);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData _EventData)
	{
		_EventData.Use();
		
		Reposition(Vector2.zero, true);
		
		m_Player.SortTrack<ASFTapTrack, ASFTapClip>();
	}

	bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 _Position, Camera _Camera)
	{
		return m_CreateTapHandle != null && m_CreateTapHandle.gameObject.activeSelf;
	}
}