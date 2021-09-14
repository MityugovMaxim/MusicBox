using System;
using System.Collections.Generic;
using System.Text;
using Firebase.Database;
using UnityEditor;
using UnityEngine;

public class ProductInfoEditor : Editor
{
	
}

[CustomEditor(typeof(LevelInfo))]
public class LevelInfoEditor : Editor
{
	SerializedProperty ActiveProperty => m_ActiveProperty ?? (m_ActiveProperty = serializedObject.FindProperty("m_Active"));
	SerializedProperty TitleProperty  => m_TitleProperty ?? (m_TitleProperty = serializedObject.FindProperty("m_Title"));
	SerializedProperty ArtistProperty => m_ArtistProperty ?? (m_ArtistProperty = serializedObject.FindProperty("m_Artist"));
	SerializedProperty IDProperty     => m_IDProperty ?? (m_IDProperty = serializedObject.FindProperty("m_ID"));
	SerializedProperty ModeProperty   => m_ModeProperty ?? (m_ModeProperty = serializedObject.FindProperty("m_Mode"));
	SerializedProperty LockedProperty => m_LockedProperty ?? (m_LockedProperty = serializedObject.FindProperty("m_Locked"));
	SerializedProperty PayoutProperty => m_PayoutProperty ?? (m_PayoutProperty = serializedObject.FindProperty("m_Payout"));
	SerializedProperty PriceProperty  => m_PriceProperty ?? (m_PriceProperty = serializedObject.FindProperty("m_Price"));
	SerializedProperty LengthProperty => m_LengthProperty ?? (m_LengthProperty = serializedObject.FindProperty("m_Length"));
	SerializedProperty BPMProperty    => m_BPMProperty ?? (m_BPMProperty = serializedObject.FindProperty("m_BPM"));
	SerializedProperty SpeedProperty  => m_SpeedProperty ?? (m_SpeedProperty = serializedObject.FindProperty("m_Speed"));
	SerializedProperty SkinProperty   => m_SkinProperty ?? (m_SkinProperty = serializedObject.FindProperty("m_Skin"));

	static string m_Config;

	SerializedProperty m_ActiveProperty;
	SerializedProperty m_TitleProperty;
	SerializedProperty m_ArtistProperty;
	SerializedProperty m_IDProperty;
	SerializedProperty m_ModeProperty;
	SerializedProperty m_LockedProperty;
	SerializedProperty m_PayoutProperty;
	SerializedProperty m_PriceProperty;
	SerializedProperty m_LengthProperty;
	SerializedProperty m_BPMProperty;
	SerializedProperty m_SpeedProperty;
	SerializedProperty m_SkinProperty;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		serializedObject.UpdateIfRequiredOrScript();
		
		EditorGUI.BeginChangeCheck();
		
		EditorGUILayout.PropertyField(ArtistProperty);
		EditorGUILayout.PropertyField(TitleProperty);
		
		if (EditorGUI.EndChangeCheck())
			IDProperty.stringValue = GetID(ArtistProperty.stringValue, TitleProperty.stringValue);
		
		EditorGUILayout.PropertyField(IDProperty);
		EditorGUILayout.PropertyField(ModeProperty);
		EditorGUILayout.PropertyField(LengthProperty);
		EditorGUILayout.PropertyField(BPMProperty);
		EditorGUILayout.PropertyField(SpeedProperty);
		EditorGUILayout.PropertyField(SkinProperty);
		
		EditorGUILayout.PropertyField(PayoutProperty);
		EditorGUILayout.PropertyField(LockedProperty);
		if (LockedProperty.boolValue)
			EditorGUILayout.PropertyField(PriceProperty);
		
		if (GUILayout.Button("Upload"))
			Upload();
		
		serializedObject.ApplyModifiedProperties();
	}

	public static string GetID(string _Artist, string _Title)
	{
		return GetData(_Artist, _Title);
	}

	public static string GetData(params string[] _Text)
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

	async void Upload()
	{
		await FirebaseAdmin.Login();
		
		DatabaseReference levels   = FirebaseDatabase.DefaultInstance.RootReference.Child("levels");
		string            levelID  = IDProperty.stringValue;
		IDictionary<string, object> data = new Dictionary<string, object>()
		{
			{ "active", ActiveProperty.boolValue },
			{ "artist", ArtistProperty.stringValue },
			{ "title", TitleProperty.stringValue },
			{ "mode", ModeProperty.intValue },
			{ "length", LengthProperty.floatValue },
			{ "bpm", BPMProperty.floatValue },
			{ "speed", SpeedProperty.floatValue },
			{ "locked", LockedProperty.boolValue },
			{ "payout", PayoutProperty.longValue },
			{ "price", PriceProperty.longValue },
			{ "skin", SkinProperty.stringValue },
		};
		try
		{
			await levels.Child(levelID).SetValueAsync(data);
			
			Debug.LogFormat("[LevelInfo] Upload level success. Level ID: {0}.", levelID);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		FirebaseAdmin.Logout();
	}
}