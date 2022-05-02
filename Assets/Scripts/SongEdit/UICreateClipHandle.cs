using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public abstract class UICreateClipHandle<TTrack, TClip> : UISeekHandle, IPointerClickHandler where TTrack : ASFTrack<TClip> where TClip : ASFClip
{
	[Inject] UIBeat m_Beat;

	readonly ClickCounter m_CreateClip = new ClickCounter(2);

	bool m_Drag;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		if (Player == null)
			return;
		
		TTrack track = Player.GetTrack<TTrack>();
		
		if (track == null || track.Context == null)
			return;
		
		track.Context.BringToFront();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		if (!m_CreateClip.Execute(_EventData))
			return;
		
		ClipSelection.Clear();
		
		TTrack track = Player.GetTrack<TTrack>();
		
		if (track == null)
			return;
		
		ASFTrackContext<TClip> context = track.Context;
		
		if (context == null)
			return;
		
		Rect area = context.GetLocalRect();
		
		Rect rect = m_Beat.GetLocalRect()
			.HorizontalResize(area.width, m_Beat.RectTransform.pivot)
			.HorizontalPadding(area.width / 8);
		
		Vector2 point = _EventData.position
			.TransformPoint(m_Beat.RectTransform)
			.HorizontalClamp(rect.xMin, rect.xMax);
		
		double minTime = m_Beat.Time + m_Beat.MinTime;
		double maxTime = m_Beat.Time + m_Beat.MaxTime;
		
		double time     = ASFMath.PositionToTime(point.y, rect.yMin, rect.yMax, minTime, maxTime);
		float  position = ASFMath.PositionToPhase(point.x, rect.xMin, rect.xMax);
		
		time     = m_Beat.Snap(time);
		position = ASFMath.SnapPhase(position, 1f / 3f);
		
		TClip clip = CreateClip(time, position);
		
		track.AddClip(clip);
		
		Player.Sample();
	}

	protected abstract TClip CreateClip(double _Time, float _Position);
}