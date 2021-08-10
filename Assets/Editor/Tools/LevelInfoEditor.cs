using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

[CustomEditor(typeof(LevelInfo))]
public class LevelInfoEditor : Editor
{
	SerializedProperty TitleProperty         => m_TitleProperty ?? (m_TitleProperty = serializedObject.FindProperty("m_Title"));
	SerializedProperty ArtistProperty        => m_ArtistProperty ?? (m_ArtistProperty = serializedObject.FindProperty("m_Artist"));
	SerializedProperty IDProperty            => m_IDProperty ?? (m_IDProperty = serializedObject.FindProperty("m_ID"));
	SerializedProperty LeaderboardIDProperty => m_LeaderboardIDProperty ?? (m_LeaderboardIDProperty = serializedObject.FindProperty("m_LeaderboardID"));
	SerializedProperty AchievementIDProperty => m_AchievementIDProperty ?? (m_AchievementIDProperty = serializedObject.FindProperty("m_AchievementID"));
	SerializedProperty ModeProperty          => m_ModeProperty ?? (m_ModeProperty = serializedObject.FindProperty("m_Mode"));
	SerializedProperty LockedProperty        => m_LockedProperty ?? (m_LockedProperty = serializedObject.FindProperty("m_Locked"));
	SerializedProperty EXPProperty           => m_EXPProperty ?? (m_EXPProperty = serializedObject.FindProperty("m_EXP"));

	SerializedProperty m_TitleProperty;
	SerializedProperty m_ArtistProperty;
	SerializedProperty m_IDProperty;
	SerializedProperty m_LeaderboardIDProperty;
	SerializedProperty m_AchievementIDProperty;
	SerializedProperty m_ModeProperty;
	SerializedProperty m_LockedProperty;
	SerializedProperty m_EXPProperty;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		serializedObject.UpdateIfRequiredOrScript();
		
		EditorGUI.BeginChangeCheck();
		
		EditorGUILayout.PropertyField(ArtistProperty);
		EditorGUILayout.PropertyField(TitleProperty);
		
		if (EditorGUI.EndChangeCheck())
		{
			IDProperty.stringValue            = GetID(ArtistProperty.stringValue, TitleProperty.stringValue);
			LeaderboardIDProperty.stringValue = GetLeaderboardID(TitleProperty.stringValue);
			AchievementIDProperty.stringValue = GetAchievementID(TitleProperty.stringValue);
		}
		
		EditorGUILayout.PropertyField(IDProperty);
		EditorGUILayout.PropertyField(LeaderboardIDProperty);
		EditorGUILayout.PropertyField(AchievementIDProperty);
		EditorGUILayout.PropertyField(ModeProperty);
		
		EditorGUILayout.PropertyField(LockedProperty);
		if (LockedProperty.boolValue)
			EditorGUILayout.PropertyField(EXPProperty);
		
		serializedObject.ApplyModifiedProperties();
	}

	static string GetID(string _Artist, string _Title)
	{
		return GetData(_Artist, _Title);
	}

	static string GetLeaderboardID(string _Title)
	{
		return GetData(_Title);
	}

	static string GetAchievementID(string _Title)
	{
		return GetData(_Title, "s_rank");
	}

	static string GetData(params string[] _Text)
	{
		List<string> words = new List<string>();
		foreach (string text in _Text)
		{
			foreach (string word in text.Split(new char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries))
			{
				StringBuilder data = new StringBuilder();
				foreach (char symbol in word)
				{
					if (char.IsLetterOrDigit(symbol))
						data.Append(char.ToLowerInvariant(symbol));
				}
				if (data.Length > 0)
					words.Add(data.ToString());
			}
		}
		return string.Join("_", words.ToArray());
	}
}