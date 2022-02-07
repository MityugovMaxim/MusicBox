using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;

[Serializable]
public class ProgressDatabaseEntry : DatabaseEntry
{
	public override string Key => m_ID;

	[SerializeField] string m_ID;
	[SerializeField] bool   m_Active;
	[SerializeField] int    m_Level;
	[SerializeField] int    m_MinLimit;
	[SerializeField] int    m_MaxLimit;
	[SerializeField] bool   m_Remove;

	public override void Draw()
	{
		EditorGUILayout.BeginHorizontal();
		
		m_Active = EditorGUILayout.Toggle(m_Active, GUILayout.Width(16));
		
		EditorGUILayout.SelectableLabel(m_ID, GUILayout.Height(18));
		
		GUI.backgroundColor = m_Remove ? Color.green : Color.red;
		if (GUILayout.Button(m_Remove ? "Restore" : "Remove", EditorStyles.miniButton, GUILayout.Width(60)))
			m_Remove = !m_Remove;
		GUI.backgroundColor = Color.white;
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUI.BeginChangeCheck();
		m_Level = EditorGUILayout.IntField("Level:", m_Level);
		if (EditorGUI.EndChangeCheck())
			m_ID = $"level_{m_Level}";
		
		m_MinLimit = EditorGUILayout.IntField("Min Limit:", m_MinLimit);
		m_MaxLimit = EditorGUILayout.IntField("Max Limit:", m_MaxLimit);
	}

	public override void Deserialize(DataSnapshot _DataSnapshot)
	{
		m_ID       = _DataSnapshot.Key;
		m_Active   = _DataSnapshot.GetBool("active");
		m_Level    = _DataSnapshot.GetInt("level");
		m_MinLimit = _DataSnapshot.GetInt("min_limit");
		m_MaxLimit = _DataSnapshot.GetInt("max_limit");
	}

	public override Dictionary<string, object> Serialize()
	{
		if (m_Remove)
			return null;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]    = m_Active;
		data["level"]     = m_Level;
		data["min_limit"] = m_MinLimit;
		data["max_limit"] = m_MaxLimit;
		data["order"]     = Order;
		
		return data;
	}
}

public class ProgressDatabaseEditorWindow : DatabaseEditorWindow<ProgressDatabaseEntry>
{
	[MenuItem("Database/Progress...")]
	public static void Open()
	{
		ProgressDatabaseEditorWindow window = GetWindow<ProgressDatabaseEditorWindow>(true, "Progress");
		window.minSize = new Vector2(640, 480);
		window.ShowUtility();
	}

	protected override DatabaseReference DatabaseReference => FirebaseDatabase.DefaultInstance.RootReference.Child("progress");

	protected override List<ProgressDatabaseEntry> Deserialize(DataSnapshot _DataSnapshot)
	{
		List<ProgressDatabaseEntry> entries = new List<ProgressDatabaseEntry>();
		
		foreach (DataSnapshot entrySnapshot in _DataSnapshot.Children)
		{
			ProgressDatabaseEntry entry = new ProgressDatabaseEntry();
			entry.Deserialize(entrySnapshot);
			entries.Add(entry);
		}
		
		return entries;
	}

	protected override string Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (ProgressDatabaseEntry entry in Entries)
			data[entry.Key] = entry.Serialize();
		
		return Json.Serialize(data);
	}
}