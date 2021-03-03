using UnityEditor;
using UnityEngine;

[ClipDrawer(typeof(EventClip))]
public class EventClipDrawer : ClipDrawer
{
	const float HANDLE_WIDTH = 5;

	protected override bool Visible
	{
		get
		{
			float min = MathUtility.Remap(
				ClipRect.center.x - HANDLE_WIDTH * 0.5f,
				TrackRect.xMin,
				TrackRect.xMax,
				TrackMinTime,
				TrackMaxTime
			);
			
			float max = MathUtility.Remap(
				ClipRect.center.x + HANDLE_WIDTH * 0.5f,
				TrackRect.xMin,
				TrackRect.xMax,
				TrackMinTime,
				TrackMaxTime
			);
			
			return min < MaxTime && max > MinTime;
		}
	}

	Rect HandleRect { get; set; }

	public EventClipDrawer(SerializedProperty _Property) : base(_Property) { }

	protected override void Draw()
	{
		HandleRect = new Rect(
			ClipRect.center.x - HANDLE_WIDTH * 0.5f,
			ClipRect.y,
			HANDLE_WIDTH,
			ClipRect.height
		);
		
		DrawBackground();
		DrawHandles();
	}

	protected override void DrawBackground()
	{
		Rect rect = new RectOffset(0, 0, 2, 2).Remove(HandleRect);
		
		Handles.DrawAAConvexPolygon(
			new Vector3(rect.center.x, rect.yMin),
			new Vector3(rect.xMin, rect.yMin + rect.width * 0.5f),
			new Vector3(rect.xMin, rect.yMax - rect.width * 0.5f),
			new Vector3(rect.center.x, rect.yMax),
			new Vector3(rect.xMax, rect.yMax - rect.width * 0.5f),
			new Vector3(rect.xMax, rect.yMin + rect.width * 0.5f)
		);
	}

	protected override void DrawHandles()
	{
		RectOffset handlePadding = new RectOffset(100, 100, 0, 0);
		
		Rect centerHandleRect = new Rect(
			HandleRect.x - 4,
			HandleRect.y,
			HandleRect.width + 8,
			HandleRect.height
		);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == CenterHandleControlID
						? handlePadding.Add(centerHandleRect)
						: centerHandleRect,
					MouseCursor.ResizeHorizontal,
					CenterHandleControlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (centerHandleRect.Contains(Event.current.mousePosition))
				{
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
						HandleRect.center.x + Event.current.delta.x,
						TrackRect.xMin,
						TrackRect.xMax,
						TrackMinTime,
						TrackMaxTime
					);
					
					time = Mathf.Max(0, time);
					
					Resize(time, time);
					
					Event.current.Use();
				}
				
				break;
			}
		}
	}
}