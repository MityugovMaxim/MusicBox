using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;

[Serializable]
public class NewsDatabaseEntry : DatabaseEntry
{
	public override string Key => m_ID ?? string.Empty;

	[SerializeField] string    m_ID;
	[SerializeField] bool      m_Active;
	[SerializeField] string    m_Language;
	[SerializeField] string    m_Title;
	[SerializeField] string    m_Description;
	[SerializeField] string    m_URL;
	[SerializeField] Texture2D m_Thumbnail;
	[SerializeField] bool      m_Remove;

	public NewsDatabaseEntry()
	{
		m_ID = FirebaseDatabase.DefaultInstance.RootReference.Child("news").Push().Key;
	}

	async void LoadThumbnail()
	{
		m_Thumbnail = await LoadTexture($"Thumbnails/News/{m_ID}.jpg");
	}

	async void UploadThumbnail()
	{
		await UploadTexture($"Thumbnails/News/{m_ID}.jpg", m_Thumbnail);
	}

	async void Upload()
	{
		await FirebaseAdmin.Login();
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child($"news/{m_ID}");
		
		if (m_Remove)
			await reference.RemoveValueAsync();
		else
			await reference.SetValueAsync(Serialize());
		
		UploadThumbnail();
	}

	public override void Draw()
	{
		EditorGUILayout.BeginHorizontal();
		
		m_Thumbnail = EditorGUILayout.ObjectField(m_Thumbnail, typeof(Texture2D), false, GUILayout.Width(60), GUILayout.Height(60)) as Texture2D;
		
		GUILayout.Space(10);
		
		EditorGUILayout.BeginVertical();
		
		EditorGUILayout.BeginHorizontal();
		
		m_Active = EditorGUILayout.Toggle(m_Active, GUILayout.Width(16));
		
		EditorGUILayout.SelectableLabel(m_ID, GUILayout.Height(18));
		
		if (GUILayout.Button("Upload", EditorStyles.miniButtonLeft, GUILayout.Width(60)))
			Upload();
		
		GUI.backgroundColor = m_Remove ? Color.green : Color.red;
		if (GUILayout.Button(m_Remove ? "Restore" : "Remove", EditorStyles.miniButtonRight, GUILayout.Width(60)))
			m_Remove = !m_Remove;
		GUI.backgroundColor = Color.white;
		
		EditorGUILayout.EndHorizontal();
		
		m_Language    = EditorGUILayout.TextField("Language:", m_Language);
		m_Title       = EditorGUILayout.TextField("Title:", m_Title);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Description:");
		GUILayout.Space(2);
		m_Description = EditorGUILayout.TextArea(m_Description, GUILayout.Height(50));
		EditorGUILayout.EndHorizontal();
		m_URL         = EditorGUILayout.TextField("URL:", m_URL);
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndHorizontal();
	}

	public override void Deserialize(DataSnapshot _DataSnapshot)
	{
		m_ID          = _DataSnapshot.Key;
		m_Active      = _DataSnapshot.GetBool("active");
		m_Language    = _DataSnapshot.GetString("language");
		m_Title       = _DataSnapshot.GetString("title");
		m_Description = _DataSnapshot.GetString("description") ?? string.Empty;
		m_URL         = _DataSnapshot.GetString("url");
		
		LoadThumbnail();
	}

	public override Dictionary<string, object> Serialize()
	{
		if (m_Remove)
			return null;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]      = m_Active;
		data["language"]    = m_Language;
		data["title"]       = m_Title;
		data["description"] = m_Description;
		data["url"]         = m_URL;
		data["order"]       = Order;
		
		return data;
	}
}

public class NewsDatabaseEditorWindow : DatabaseEditorWindow<NewsDatabaseEntry>
{
	[MenuItem("Database/News...")]
	public static void Open()
	{
		NewsDatabaseEditorWindow window = GetWindow<NewsDatabaseEditorWindow>(true, "News");
		window.minSize = new Vector2(640, 480);
		window.ShowUtility();
	}

	protected override DatabaseReference DatabaseReference => FirebaseDatabase.DefaultInstance.RootReference.Child("news");

	protected override List<NewsDatabaseEntry> Deserialize(DataSnapshot _DataSnapshot)
	{
		List<NewsDatabaseEntry> entries = new List<NewsDatabaseEntry>();
		
		foreach (DataSnapshot entrySnapshot in _DataSnapshot.Children)
		{
			NewsDatabaseEntry entry = new NewsDatabaseEntry();
			entry.Deserialize(entrySnapshot);
			entries.Add(entry);
		}
		
		return entries;
	}

	protected override string Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (NewsDatabaseEntry entry in Entries)
			data[entry.Key] = entry.Serialize();
		
		return Json.Serialize(data);
	}
}
