using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIMenu), true)]
public class UIMenuEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		EditorGUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Show", EditorStyles.miniButtonLeft))
			ShowMenu();
		
		if (GUILayout.Button("Hide", EditorStyles.miniButtonLeft))
			HideMenu();
		
		EditorGUILayout.EndHorizontal();
	}

	void ShowMenu()
	{
		UIMenu menu = target as UIMenu;
		
		if (menu == null)
			return;
		
		menu.Show(!Application.isPlaying);
	}

	void HideMenu()
	{
		UIMenu menu = target as UIMenu;
		
		if (menu == null)
			return;
		
		menu.Hide(!Application.isPlaying);
	}
}