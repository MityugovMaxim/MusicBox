using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(InputClip))]
public class InputClipDrawer : ClipDrawer
{
	static readonly Color m_ValidColor   = new Color(0, 0.8f, 0.7f);
	static readonly Color m_InvalidColor = new Color(1, 0.7f, 0);

	InputClip InputClip { get; }

	public InputClipDrawer(Clip _Clip) : base(_Clip)
	{
		InputClip = Clip as InputClip;
	}

	protected override void DrawBackground()
	{
		EditorGUI.DrawRect(ClipRect, new Color(0.12f, 0.12f, 0.12f, 0.5f));
		
		DrawZone();
		
		AudioCurveRendering.DrawCurveFrame(ClipRect);
	}

	protected override void DrawContent() { }

	protected override void DrawHandles()
	{
		RectOffset handlePadding = new RectOffset(100, 100, 0, 0);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == CenterHandleControlID
						? handlePadding.Add(ClipRect)
						: ClipRect,
					MouseCursor.Pan,
					CenterHandleControlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (ClipRect.Contains(Event.current.mousePosition))
				{
					Event.current.SetPosition(ClipRect.x + ClipRect.width * InputClip.ZoneTime);
					
					GUIUtility.hotControl = CenterHandleControlID;
					
					Event.current.Use();
				}
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl == CenterHandleControlID)
				{
					float time = MathUtility.Remap(
						Event.current.GetHorizontalPosition(),
						TrackRect.xMin,
						TrackRect.xMax,
						TrackMinTime,
						TrackMaxTime
					);
					
					if (Event.current.command)
						time = SnapTime(time);
					time = Mathf.Max(0, time);
					
					float duration = MaxTime - MinTime;
					
					Resize(
						time - duration * InputClip.ZoneTime,
						time + duration * (1 - InputClip.ZoneTime)
					);
					
					Event.current.Use();
				}
				
				break;
			}
		}
	}

	void DrawZone()
	{
		float zoneTimePosition = MathUtility.RemapClamped(InputClip.ZoneTime, 0, 1, ClipRect.xMin, ClipRect.xMax);
		float minZonePosition  = MathUtility.RemapClamped(InputClip.MinZone, 0, 1, ClipRect.xMin, ClipRect.xMax);
		float maxZonePosition  = MathUtility.RemapClamped(InputClip.MaxZone, 0, 1, ClipRect.xMin, ClipRect.xMax);
		
		Rect timeRect = new Rect(
			zoneTimePosition - 1,
			ClipRect.y,
			2,
			ClipRect.height
		);
		
		Rect zoneRect = new Rect(
			minZonePosition,
			ClipRect.y,
			maxZonePosition - minZonePosition,
			ClipRect.height
		);
		
		Color color = InputClip.MinZone <= InputClip.ZoneTime && InputClip.MaxZone >= InputClip.ZoneTime
			? m_ValidColor
			: m_InvalidColor;
		
		color.a = 0.5f;
		
		EditorGUI.DrawRect(zoneRect, new Color(color.r, color.g, color.b, 0.3f));
		
		EditorGUI.DrawRect(timeRect, color);
	}
}