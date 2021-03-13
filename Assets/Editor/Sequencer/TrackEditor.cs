using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Track), true)]
public class TrackEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		if (GUILayout.Button("Sort") && target is Track track)
			track.Sort();
	}
}