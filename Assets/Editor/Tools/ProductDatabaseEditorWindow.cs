using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;

[Serializable]
public class ProductDatabaseEntry : DatabaseEntry
{
	public override string Key => m_ID ?? string.Empty;

	[SerializeField] string       m_ID;
	[SerializeField] bool         m_Active;
	[SerializeField] ProductType  m_Type;
	[SerializeField] bool         m_Promo;
	[SerializeField] bool         m_NoAds;
	[SerializeField] long         m_Coins;
	[SerializeField] float        m_Discount;
	[SerializeField] List<string> m_LevelIDs;
	[SerializeField] Texture2D    m_Thumbnail;
	[SerializeField] bool         m_Remove;

	ReorderableList m_LevelIDsList;

	public ProductDatabaseEntry()
	{
		m_LevelIDs = new List<string>();
		
		CreateLevelIDs();
	}

	async void LoadThumbnail()
	{
		m_Thumbnail = await LoadTexture($"Thumbnails/Products/{m_ID}.jpg");
	}

	async void UploadThumbnail()
	{
		await UploadTexture($"Thumbnails/Products/{m_ID}.jpg", m_Thumbnail);
	}

	async void Upload()
	{
		await FirebaseAdmin.Login();
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child($"products/{m_ID}");
		
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
		
		m_ID = EditorGUILayout.TextField(m_ID, GUILayout.Height(18));
		
		if (GUILayout.Button("Upload", EditorStyles.miniButtonLeft, GUILayout.Width(60)))
			Upload();
		
		GUI.backgroundColor = m_Remove ? Color.green : Color.red;
		if (GUILayout.Button(m_Remove ? "Restore" : "Remove", EditorStyles.miniButtonRight, GUILayout.Width(60)))
			m_Remove = !m_Remove;
		GUI.backgroundColor = Color.white;
		
		EditorGUILayout.EndHorizontal();
		
		m_Type = (ProductType)EditorGUILayout.EnumPopup("Type:", m_Type);
		
		m_Promo = EditorGUILayout.Toggle("Promo:", m_Promo);
		
		m_NoAds = EditorGUILayout.Toggle("No Ads:", m_NoAds);
		
		m_Coins = EditorGUILayout.LongField("Coins:", m_Coins);
		
		m_Discount = EditorGUILayout.FloatField("Discount:", m_Discount);
		
		if (m_LevelIDsList != null)
			m_LevelIDsList.DoLayoutList();
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndHorizontal();
	}

	void CreateLevelIDs()
	{
		m_LevelIDsList = new ReorderableList(m_LevelIDs, typeof(string), true, true, true, true);
		
		m_LevelIDsList.drawHeaderCallback += _Rect =>
		{
			EditorGUI.LabelField(_Rect, "Levels", EditorStyles.whiteBoldLabel);
		};
		
		m_LevelIDsList.drawElementCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			m_LevelIDs[_Index] = EditorGUI.TextField(_Rect, m_LevelIDs[_Index]);
		};
		
		m_LevelIDsList.onAddCallback += _List =>
		{
			m_LevelIDs.Add(string.Empty);
		};
	}

	public override void Deserialize(DataSnapshot _DataSnapshot)
	{
		m_ID       = _DataSnapshot.Key;
		m_Active   = _DataSnapshot.GetBool("active");
		m_Promo    = _DataSnapshot.GetBool("promo");
		m_NoAds    = _DataSnapshot.GetBool("no_ads");
		m_Coins    = _DataSnapshot.GetLong("coins");
		m_Discount = _DataSnapshot.GetFloat("discount");
		m_LevelIDs = _DataSnapshot.GetChildKeys("levels");
		
		LoadThumbnail();
	}

	public override Dictionary<string, object> Serialize()
	{
		if (m_Remove)
			return null;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]   = m_Active;
		data["type"]     = (int)m_Type;
		data["promo"]    = m_Promo;
		data["no_ads"]   = m_NoAds;
		data["coins"]    = m_Coins;
		data["discount"] = m_Discount;
		data["levels"]   = m_LevelIDs.SkipWhile(string.IsNullOrEmpty).ToDictionary(_LevelID => _LevelID, _LevelID => true);
		data["order"]    = Order;
		
		return data;
	}
}

public class ProductDatabaseEditorWindow : DatabaseEditorWindow<ProductDatabaseEntry>
{
	[MenuItem("Database/Products...")]
	public static void Open()
	{
		ProductDatabaseEditorWindow window = GetWindow<ProductDatabaseEditorWindow>(true, "Products");
		window.minSize = new Vector2(640, 480);
		window.ShowUtility();
	}

	protected override DatabaseReference DatabaseReference => FirebaseDatabase.DefaultInstance.RootReference.Child("products");

	protected override List<ProductDatabaseEntry> Deserialize(DataSnapshot _DataSnapshot)
	{
		List<ProductDatabaseEntry> entries = new List<ProductDatabaseEntry>();
		
		foreach (DataSnapshot entrySnapshot in _DataSnapshot.Children)
		{
			ProductDatabaseEntry entry = new ProductDatabaseEntry();
			entry.Deserialize(entrySnapshot);
			entries.Add(entry);
		}
		
		return entries;
	}

	protected override string Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (ProductDatabaseEntry entry in Entries)
			data[entry.Key] = entry.Serialize();
		
		return Json.Serialize(data);
	}
}