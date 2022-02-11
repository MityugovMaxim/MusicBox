using System.Linq;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIGroup), true)]
public class UIGroupEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		GUILayout.BeginHorizontal();
		
		GUILayout.Space(20);
		
		if (GUILayout.Button("SHOW"))
		{
			Hide();
			Show();
		}
		
		if (GUILayout.Button("HIDE"))
		{
			Show();
			Hide();
		}
		
		GUILayout.Space(20);
		
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