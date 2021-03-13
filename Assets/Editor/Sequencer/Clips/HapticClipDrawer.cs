using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(HapticClip))]
public class HapticClipDrawer : ClipDrawer
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

	SerializedProperty HapticTypeProperty { get; }

	Haptic.Type HapticType
	{
		get => (Haptic.Type)HapticTypeProperty.intValue;
		set => HapticTypeProperty.intValue = (int)value;
	}

	Rect HandleRect { get; set; }

	public HapticClipDrawer(Clip _Clip) : base(_Clip)
	{
		HapticTypeProperty = ClipObject.FindProperty("m_HapticType");
	}

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
		
		Color color;
		switch (HapticType)
		{
			case Haptic.Type.Selection:
				color = new Color(0.25f, 0.6f, 0.85f);
				break;
			case Haptic.Type.Success:
				color = new Color(1, 1, 1);
				break;
			case Haptic.Type.Warning:
				color = new Color(1, 0.7f, 0);
				break;
			case Haptic.Type.Failure:
				color = new Color(0.85f, 0.35f, 0.35f);
				break;
			case Haptic.Type.ImpactLight:
				color = new Color(0, 0.8f, 0.7f, 0.35f);
				break;
			case Haptic.Type.ImpactMedium:
				color = new Color(0, 0.8f, 0.7f, 0.65f);
				break;
			case Haptic.Type.ImpactHeavy:
				color = new Color(0, 0.8f, 0.7f);
				break;
			default:
				color = new Color(1, 1, 1);
				break;
		}
		
		Handles.color = color;
		
		Handles.DrawAAConvexPolygon(
			new Vector3(rect.center.x, rect.yMin),
			new Vector3(rect.xMin, rect.yMin + rect.width * 0.5f),
			new Vector3(rect.xMin, rect.yMax - rect.width * 0.5f),
			new Vector3(rect.center.x, rect.yMax),
			new Vector3(rect.xMax, rect.yMax - rect.width * 0.5f),
			new Vector3(rect.xMax, rect.yMin + rect.width * 0.5f)
		);
		
		Handles.color = Color.white;
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
					Handles.DrawLines(
						new Vector3[]
						{
							new Vector3(rect.center.x, rect.yMin - 1),
							new Vector3(rect.xMin - 1, rect.yMin + rect.width * 0.5f - 1),
							new Vector3(rect.xMin - 1, rect.yMax - rect.width * 0.5f + 1),
							new Vector3(rect.center.x, rect.yMax + 1),
							new Vector3(rect.xMax + 1, rect.yMax - rect.width * 0.5f + 1),
							new Vector3(rect.xMax + 1, rect.yMin + rect.width * 0.5f - 1),
						},
						new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 0 }
					);
					Handles.color = Color.white;
				}
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (handleRect.Contains(Event.current.mousePosition))
					Selection.activeObject = Clip;
				
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
					
					if (Event.current.command)
						time = SnapTime(time);
					time = Mathf.Max(0, time);
					
					Resize(time, time);
					
					Event.current.Use();
				}
				
				break;
			}
		}
	}
}