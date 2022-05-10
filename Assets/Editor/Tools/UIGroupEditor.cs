using System.Linq;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIGroup), true)]
public class UIGroupEditor : Editor
{
	bool Shown => target is UIGroup group && group.Shown;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		GUILayout.BeginHorizontal();
		
		GUILayout.Space(20);
		
		GUI.backgroundColor = Shown ? Color.gray : Color.white;
		if (GUILayout.Button("SHOW"))
		{
			Hide();
			Show();
			Save();
		}
		
		GUI.backgroundColor = Shown ? Color.white : Color.gray;
		if (GUILayout.Button("HIDE"))
		{
			Show();
			Hide();
			Save();
		}
		
		GUI.backgroundColor = Color.white;
		
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

	void Save()
	{
		foreach (UIGroup group in targets.OfType<UIGroup>())
			EditorUtility.SetDirty(group);
	}
}