using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIGroup), true), CanEditMultipleObjects]
public class UIGroupEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("SHOW"))
			Show();
		
		if (GUILayout.Button("HIDE"))
			Hide();
		
		GUILayout.EndHorizontal();
	}

	void Show()
	{
		foreach (UIGroup group in targets.OfType<UIGroup>())
			group.Show(!Application.isPlaying);
	}

	void Hide()
	{
		foreach (UIGroup group in targets.OfType<UIGroup>())
			group.Hide(!Application.isPlaying);
	}
}