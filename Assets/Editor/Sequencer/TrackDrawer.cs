using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TrackDrawer
{
	static readonly Dictionary<Type, Type> m_TrackDrawerTypes = new Dictionary<Type, Type>();

	public static TrackDrawer Create(Track _Track)
	{
		if (_Track == null)
			return null;
		
		Type trackType = _Track.GetType();
		
		Type trackDrawerType = GetClipDrawerType(trackType);
		
		return Activator.CreateInstance(
			trackDrawerType,
			new object[] { _Track }
		) as TrackDrawer;
	}

	static Type GetClipDrawerType(Type _TrackType)
	{
		if (m_TrackDrawerTypes.ContainsKey(_TrackType) && m_TrackDrawerTypes[_TrackType] != null)
			return m_TrackDrawerTypes[_TrackType];
		
		Assembly assembly = typeof(TrackDrawer).Assembly;
		
		IEnumerable<Type> trackDrawerTypes = assembly.GetTypes().Where(_Type => _Type.IsSubclassOf(typeof(TrackDrawer)));
		
		foreach (Type trackDrawerType in trackDrawerTypes)
		{
			SequencerDrawerAttribute attribute = trackDrawerType.GetCustomAttribute<SequencerDrawerAttribute>();
			
			if (attribute.Type == _TrackType)
			{
				m_TrackDrawerTypes[_TrackType] = trackDrawerType;
				
				return trackDrawerType;
			}
		}
		
		return typeof(TrackDrawer);
	}

	protected Track            Track           { get; }
	protected SerializedObject TrackObject     { get; }
	protected int              HandleControlID { get; }

	protected Rect  TrackRect   { get; private set; }
	protected Rect  ContentRect { get; private set; }
	protected float Time        { get; private set; }

	public TrackDrawer(Track _Track)
	{
		Track       = _Track;
		TrackObject = new SerializedObject(_Track);
		
		int controlID = _Track.GetInstanceID();
		
		HandleControlID = GUIUtility.GetControlID($"[{controlID}]track_handle_control".GetHashCode(), FocusType.Passive);
	}

	public void Draw(Rect _TrackRect, float _Time)
	{
		TrackObject.UpdateIfRequiredOrScript();
		
		TrackRect = _TrackRect;
		Time      = _Time;
		
		ContentRect = new RectOffset(15, 15, 4, 4).Remove(TrackRect);
		
		DrawBackground();
		
		GUILayout.BeginArea(ContentRect);
		
		DrawContent();
		
		GUILayout.EndArea();
		
		DrawHandles();
		
		TrackObject.ApplyModifiedProperties();
	}

	protected void DrawName()
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
			GUI.FocusControl(null);
		
		string trackName = EditorGUILayout.DelayedTextField(Track.name, EditorStyles.boldLabel);
		
		if (Track.name == trackName)
			return;
		
		string path = AssetDatabase.GetAssetPath(Track);
		
		Track.name = trackName;
		
		AssetDatabase.RenameAsset(path, trackName);
	}

	protected virtual void DrawBackground()
	{
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUI.DrawRect(TrackRect, new Color(0.12f, 0.12f, 0.12f));
				
				AudioCurveRendering.DrawCurveFrame(TrackRect);
				
				break;
			}
		}
	}

	protected virtual void DrawContent()
	{
		DrawName();
	}

	protected virtual void DrawHandles()
	{
		RectOffset handlePadding = new RectOffset(0, 0, 100, 100);
		
		Rect handleRect = new Rect(
			TrackRect.x,
			TrackRect.yMax - 4,
			TrackRect.width,
			8
		);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == HandleControlID
						? handlePadding.Add(handleRect)
						: handleRect,
					MouseCursor.ResizeVertical,
					HandleControlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (!handleRect.Contains(Event.current.mousePosition))
					break;
				
				GUIUtility.hotControl = HandleControlID;
				
				Event.current.SetPosition(TrackRect.yMax);
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl != HandleControlID)
					break;
				
				float padding = Track.Height - TrackRect.height;
				
				Track.Height = Event.current.GetVerticalPosition() - TrackRect.yMin + padding;
				
				Event.current.Use();
				
				break;
			}
		}
	}
}