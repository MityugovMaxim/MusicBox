using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;

[Serializable]
public class OfferDatabaseEntry : DatabaseEntry
{
	public override string Key => m_ID ?? string.Empty;

	[SerializeField] string    m_ID;
	[SerializeField] bool      m_Active;
	[SerializeField] string    m_Title;
	[SerializeField] string    m_LevelID;
	[SerializeField] long      m_Coins;
	[SerializeField] int       m_AdsCount;
	[SerializeField] Texture2D m_Thumbnail;
	[SerializeField] bool      m_Remove;

	public OfferDatabaseEntry()
	{
		m_ID = FirebaseDatabase.DefaultInstance.RootReference.Child("offers").Push().Key;
	}

	async void LoadThumbnail()
	{
		m_Thumbnail = await LoadTexture($"Thumbnails/Offers/{m_ID}.jpg");
	}

	async void UploadThumbnail()
	{
		await UploadTexture($"Thumbnails/Offers/{m_ID}.jpg", m_Thumbnail);
	}

	async void Upload()
	{
		await FirebaseAdmin.Login();
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child($"offers/{m_ID}");
		
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
		
		m_Title = EditorGUILayout.TextField("Title:", m_Title);
		
		m_LevelID = EditorGUILayout.TextField("Level ID:", m_LevelID);
		
		m_Coins = EditorGUILayout.LongField("Coins:", m_Coins);
		
		m_AdsCount = EditorGUILayout.IntField("Ads Count:", m_AdsCount);
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndHorizontal();
	}

	public override void Deserialize(DataSnapshot _DataSnapshot)
	{
		m_ID       = _DataSnapshot.Key;
		m_Active   = _DataSnapshot.GetBool("active");
		m_Title    = _DataSnapshot.GetString("title");
		m_LevelID  = _DataSnapshot.GetString("level_id");
		m_Coins    = _DataSnapshot.GetLong("coins");
		m_AdsCount = _DataSnapshot.GetInt("ads_count");
		
		LoadThumbnail();
	}

	public override Dictionary<string, object> Serialize()
	{
		if (m_Remove)
			return null;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]    = m_Active;
		data["title"]     = m_Title;
		data["level_id"]  = m_LevelID;
		data["coins"]     = m_Coins;
		data["ads_count"] = m_AdsCount;
		data["order"]     = Order;
		
		return data;
	}
}

public class OffersDatabaseEditorWindow : DatabaseEditorWindow<OfferDatabaseEntry>
{
	[MenuItem("Database/Offers...")]
	public static void Open()
	{
		OffersDatabaseEditorWindow window = GetWindow<OffersDatabaseEditorWindow>(true, "Offers");
		window.minSize = new Vector2(640, 480);
		window.ShowUtility();
	}

	protected override DatabaseReference DatabaseReference => FirebaseDatabase.DefaultInstance.RootReference.Child("offers");

	protected override List<OfferDatabaseEntry> Deserialize(DataSnapshot _DataSnapshot)
	{
		List<OfferDatabaseEntry> offers = new List<OfferDatabaseEntry>();
		
		foreach (DataSnapshot offerSnapshot in _DataSnapshot.Children)
		{
			OfferDatabaseEntry offer = new OfferDatabaseEntry();
			offer.Deserialize(offerSnapshot);
			offers.Add(offer);
		}
		
		return offers;
	}

	protected override string Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (OfferDatabaseEntry entry in Entries)
			data[entry.Key] = entry.Serialize();
		
		return Json.Serialize(data);
	}
}