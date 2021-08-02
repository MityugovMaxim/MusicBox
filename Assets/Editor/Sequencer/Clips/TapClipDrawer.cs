using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(TapClip))]
public class TapClipDrawer : ClipDrawer
{
	const float HANDLE_WIDTH = 10;

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

	public TapClipDrawer(Clip _Clip) : base(_Clip) { }

	protected override void Draw()
	{
		HandleRect = new Rect(
			ClipRect.center.x - HANDLE_WIDTH * 0.5f,
			ClipRect.y,
			HANDLE_WIDTH,
			ClipRect.height
		);
		
		base.Draw();
	}

	protected override void DrawBackground()
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		Rect rect = new RectOffset(0, 0, 2, 2).Remove(HandleRect);
		
		Handles.DrawWireDisc(
			rect.center,
			Vector3.back,
			rect.width * 0.5f
		);
		
		Handles.DrawSolidDisc(
			rect.center,
			Vector3.back,
			2
		);
	}

	protected override void DrawSelection()
	{
		Rect handleRect = new Rect(
			HandleRect.x - 4,
			HandleRect.y,
			HandleRect.width + 8,
			HandleRect.height
		);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				if (Selection.Contains(Clip))
				{
					Rect rect = new RectOffset(0, 0, 2, 2).Remove(HandleRect);
					
					Handles.color = new Color(0.25f, 0.6f, 0.85f);
					Handles.DrawWireDisc(
						rect.center,
						Vector3.back,
						rect.width * 0.5f
					);
					
					Handles.DrawSolidDisc(
						rect.center,
						Vector3.back,
						2
					);
					Handles.color = Color.white;
				}
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (handleRect.Contains(Event.current.mousePosition))
				{
					GUI.FocusControl(null);
					Selection.activeObject = Clip;
				}
				
				break;
			}
		}
	}

	protected override void DrawHandles()
	{
		RectOffset handlePadding = new RectOffset(100, 100, 0, 0);
		
		Rect handleRect = new Rect(
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
						? handlePadding.Add(handleRect)
						: handleRect,
					MouseCursor.ResizeHorizontal,
					CenterHandleControlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (handleRect.Contains(Event.current.mousePosition))
				{
					Event.current.SetPosition(ClipRect);
					
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
						Event.current.GetPosition().x,
						TrackRect.xMin,
						TrackRect.xMax,
						TrackMinTime,
						TrackMaxTime
					);
					
					if (Event.current.modifiers == EventModifiers.Command)
						time = SnapTime(time);
					else if (Event.current.modifiers == EventModifiers.Control)
						time = SnapBPM(time);
					time = Mathf.Max(0, time);
					
					Resize(time, time);
					
					Event.current.Use();
				}
				
				break;
			}
			
			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl != CenterHandleControlID)
					break;
				
				Reposition();
				
				GUIUtility.hotControl = 0;
				
				Event.current.Use();
				
				break;
			}
		}
	}
}