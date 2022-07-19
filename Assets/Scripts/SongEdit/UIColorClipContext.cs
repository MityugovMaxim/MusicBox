using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UIColorClipContext : ASFClipContext<ASFColorClip>, IPointerClickHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
{
	[Preserve]
	public class Pool : MonoMemoryPool<RectTransform, ASFColorClip, Rect, Rect, UIColorClipContext>
	{
		protected override void Reinitialize(RectTransform _Container, ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect, UIColorClipContext _Item)
		{
			_Item.Setup(_Container, _Clip, _ClipRect, _ViewRect);
			
			_Item.Select(ClipSelection.Contains(_Item.Clip));
		}

		protected override void OnSpawned(UIColorClipContext _Item)
		{
			base.OnSpawned(_Item);
			
			ClipSelection.Changed += _Item.OnSelectionChanged;
		}

		protected override void OnDespawned(UIColorClipContext _Item)
		{
			base.OnDespawned(_Item);
			
			ClipSelection.Changed -= _Item.OnSelectionChanged;
		}
	}

	[SerializeField] GameObject m_Selection;
	[SerializeField] Graphic    m_BackgroundPrimary;
	[SerializeField] Graphic    m_BackgroundSecondary;
	[SerializeField] Graphic    m_ForegroundPrimary;
	[SerializeField] Graphic    m_ForegroundSecondary;

	[Inject] UIBeat              m_Beat;
	[Inject] UIPlayer            m_Player;
	[Inject] UICreateColorHandle m_CreateColorHandle;
	[Inject] MenuProcessor       m_MenuProcessor;

	readonly ClickCounter m_EditColor = new ClickCounter(2);

	public override void Setup(RectTransform _Container, ASFColorClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		base.Setup(_Container, _Clip, _ClipRect, _ViewRect);
		
		m_BackgroundPrimary.color   = _Clip.BackgroundPrimary;
		m_BackgroundSecondary.color = _Clip.BackgroundSecondary;
		m_ForegroundPrimary.color   = _Clip.ForegroundPrimary;
		m_ForegroundSecondary.color = _Clip.ForegroundSecondary;
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

	void ProcessColors(
		Color _BackgroundPrimary,
		Color _BackgroundSecondary,
		Color _ForegroundPrimary,
		Color _ForegroundSecondary
	)
	{
		Clip.BackgroundPrimary   = _BackgroundPrimary;
		Clip.BackgroundSecondary = _BackgroundSecondary;
		Clip.ForegroundPrimary   = _ForegroundPrimary;
		Clip.ForegroundSecondary = _ForegroundSecondary;
		
		m_BackgroundPrimary.color   = Clip.BackgroundPrimary;
		m_BackgroundSecondary.color = Clip.BackgroundSecondary;
		m_ForegroundPrimary.color   = Clip.ForegroundPrimary;
		m_ForegroundSecondary.color = Clip.ForegroundSecondary;
		
		m_Player.Sample();
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
		
		m_Player.SortTrack<ASFColorTrack, ASFColorClip>();
	}

	bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 _Position, Camera _Camera)
	{
		return m_CreateColorHandle != null && m_CreateColorHandle.gameObject.activeSelf;
	}

	async void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		if (!m_EditColor.Execute(_EventData))
			return;
		
		_EventData.Use();
		
		UIColorMenu colorMenu = m_MenuProcessor.GetMenu<UIColorMenu>();
		
		colorMenu.Setup(Clip, ProcessColors);
		
		await m_MenuProcessor.Show(MenuType.ColorMenu);
	}
}