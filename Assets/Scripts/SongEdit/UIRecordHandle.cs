using System.Collections.Generic;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIRecordHandle : UIEntity, IPointerDownHandler, IPointerUpHandler
{
	class RecordData
	{
		public double MinTime;
		public double MaxTime;
		public float  MinPosition;
		public float  MaxPosition;
	}

	[Inject] UIBeat   m_Beat;
	[Inject] UIPlayer m_Player;

	readonly Dictionary<int, RecordData> m_Records = new Dictionary<int, RecordData>();

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		ClipSelection.Clear();
		
		Rect area = m_Player.GetLocalRect();
		
		Rect rect = m_Beat.GetLocalRect()
		
			.HorizontalResize(area.width, m_Beat.RectTransform.pivot)
			.HorizontalPadding(area.width / 8);
		
		Vector2 point = _EventData.position
			.TransformPoint(m_Beat.RectTransform)
			.HorizontalClamp(rect.xMin, rect.xMax);
		
		float position = ASFMath.PositionToPhase(point.x, rect.xMin, rect.xMax);
		
		RecordData data = new RecordData();
		
		data.MinTime = m_Player.Time;
		data.MaxTime = m_Player.Time;
		
		data.MinPosition = position;
		data.MaxPosition = position;
		
		m_Records[_EventData.pointerId] = data;
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		if (!m_Records.TryGetValue(_EventData.pointerId, out RecordData data) || data == null)
			return;
		
		Rect area = m_Player.GetLocalRect();
		
		Rect rect = m_Beat.GetLocalRect()
			.HorizontalResize(area.width, m_Beat.RectTransform.pivot)
			.HorizontalPadding(area.width / 8);
		
		Vector2 point = _EventData.position
			.TransformPoint(m_Beat.RectTransform)
			.HorizontalClamp(rect.xMin, rect.xMax);
		
		data.MaxTime = m_Player.Time;
		
		data.MaxPosition = ASFMath.PositionToPhase(point.x, rect.xMin, rect.xMax);
		
		double length = data.MaxTime - data.MinTime;
		
		if (length < m_Beat.Step * 2)
		{
			ASFTapTrack tapTrack = m_Player.GetTrack<ASFTapTrack>();
			
			double tapTime     = m_Beat.Snap(data.MinTime);
			float  tapPosition = ASFMath.SnapPhase(data.MinPosition, 1f / 3f);
			
			ASFTapClip tapClip = new ASFTapClip(tapTime, tapPosition);
			
			tapTrack.AddClip(tapClip);
		}
		else
		{
			ASFHoldTrack holdTrack = m_Player.GetTrack<ASFHoldTrack>();
			
			double holdMinTime = m_Beat.Snap(data.MinTime);
			double holdMaxTime = m_Beat.Snap(data.MaxTime);
			double holdLength  = holdMaxTime - holdMinTime;
			
			float holdMinPosition = ASFMath.SnapPhase(data.MinPosition, 1f / 3f);
			float holdMaxPosition = ASFMath.SnapPhase(data.MaxPosition, 1f / 3f);
			
			ASFHoldClip holdClip = new ASFHoldClip(
				holdMinTime,
				holdMaxTime,
				new ASFHoldKey(0, holdMinPosition),
				new ASFHoldKey(holdLength, holdMaxPosition)
			);
			
			holdTrack.AddClip(holdClip);
		}
		
		m_Records.Remove(_EventData.pointerId);
		
		m_Player.Sample();
	}
}