using System;
using System.Collections.Generic;
using System.Text;
using Firebase.Database;
using Firebase.Storage;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using Object = UnityEngine.Object;

[Serializable]
public class LevelDatabaseEntry : DatabaseEntry
{
	public override string Key => m_ID;

	[SerializeField] string    m_ID;
	[SerializeField] bool      m_Active;
	[SerializeField] string    m_Title;
	[SerializeField] string    m_Artist;
	[SerializeField] LevelMode m_Mode;
	[SerializeField] int       m_Level;
	[SerializeField] long      m_Price;
	[SerializeField] long      m_DefaultPayout;
	[SerializeField] long      m_BronzePayout;
	[SerializeField] long      m_SilverPayout;
	[SerializeField] long      m_GoldPayout;
	[SerializeField] long      m_PlatinumPayout;
	[SerializeField] float     m_Length;
	[SerializeField] float     m_BPM;
	[SerializeField] float     m_Speed;
	[SerializeField] string    m_Skin;
	[SerializeField] Texture2D m_Thumbnail;
	[SerializeField] AudioClip m_Preview;
	[SerializeField] bool      m_PayoutsFoldout;
	[SerializeField] bool      m_SettingsFoldout;
	[SerializeField] Object    m_SkinAsset;
	[SerializeField] bool      m_Remove;

	async void LoadThumbnail()
	{
		m_Thumbnail = await LoadTexture($"Thumbnails/Levels/{m_ID}.jpg");
	}

	async void UploadThumbnail()
	{
		await UploadTexture($"Thumbnails/Levels/{m_ID}.jpg", m_Thumbnail);
	}

	async void LoadPreview()
	{
		await FirebaseAdmin.Login();
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child($"Previews/{m_ID}.ogg");
		
		Uri uri = await reference.GetDownloadUrlAsync();
		
		m_Preview      = await WebRequest.LoadAudioClip(uri.ToString(), AudioType.OGGVORBIS);
		m_Preview.name = m_ID;
	}

	async void UploadPreview()
	{
		string path = AssetDatabase.GetAssetPath(m_Preview);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		await FirebaseAdmin.Login();
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child($"Previews/{m_ID}.ogg");
		
		await reference.PutFileAsync(path);
	}

	void LoadSkinAsset()
	{
		m_SkinAsset = Resources.Load<Level>(m_Skin);
	}

	string GenerateID(string _Artist, string _Title)
	{
		return GetData(_Artist, _Title);
	}

	string GetData(params string[] _Text)
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
		
		EditorGUI.BeginChangeCheck();
		
		m_Artist = EditorGUILayout.TextField("Artist:", m_Artist);
		
		m_Title = EditorGUILayout.TextField("Title:", m_Title);
		
		if (EditorGUI.EndChangeCheck())
			m_ID = GenerateID(m_Artist, m_Title);
		
		m_Mode = (LevelMode)EditorGUILayout.EnumPopup("Mode:", m_Mode);
		
		m_Level = EditorGUILayout.IntField("Level:", m_Level);
		
		m_Price = EditorGUILayout.LongField("Price:", m_Price);
		
		m_Preview = EditorGUILayout.ObjectField("Preview:", m_Preview, typeof(AudioClip), false) as AudioClip;
		
		m_PayoutsFoldout = EditorGUILayout.Foldout(m_PayoutsFoldout, "Payouts", true);
		
		if (m_PayoutsFoldout)
		{
			EditorGUI.indentLevel++;
			
			m_DefaultPayout  = EditorGUILayout.LongField("Default payout:", m_DefaultPayout);
			m_BronzePayout   = EditorGUILayout.LongField("Bronze payout:", m_BronzePayout);
			m_SilverPayout   = EditorGUILayout.LongField("Silver payout:", m_SilverPayout);
			m_GoldPayout     = EditorGUILayout.LongField("Gold payout:", m_GoldPayout);
			m_PlatinumPayout = EditorGUILayout.LongField("Platinum payout:", m_PlatinumPayout);
			EditorGUI.indentLevel--;
		}
		
		m_SettingsFoldout = EditorGUILayout.Foldout(m_SettingsFoldout, "Settings", true);
		
		if (m_SettingsFoldout)
		{
			EditorGUI.indentLevel++;
			m_Length    = EditorGUILayout.FloatField("Length:", m_Length);
			m_BPM       = EditorGUILayout.FloatField("BPM:", m_BPM);
			m_Speed     = EditorGUILayout.FloatField("Speed:", m_Speed);
			m_SkinAsset = EditorGUILayout.ObjectField("Skin:", m_SkinAsset, typeof(Level), false) as Level;
			EditorGUI.indentLevel--;
		}
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndHorizontal();
	}

	async void Upload()
	{
		await FirebaseAdmin.Login();
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child($"levels/{m_ID}");
		
		if (m_Remove)
			await reference.RemoveValueAsync();
		else
			await reference.SetValueAsync(Serialize());
		
		UploadThumbnail();
		
		UploadPreview();
	}

	public override void Deserialize(DataSnapshot _DataSnapshot)
	{
		m_ID             = _DataSnapshot.Key;
		m_Active         = _DataSnapshot.GetBool("active");
		m_Title          = _DataSnapshot.GetString("title");
		m_Artist         = _DataSnapshot.GetString("artist");
		m_Level          = _DataSnapshot.GetInt("level");
		m_Price          = _DataSnapshot.GetLong("price");
		m_Mode           = _DataSnapshot.GetEnum<LevelMode>("mode");
		m_Length         = _DataSnapshot.GetFloat("length");
		m_BPM            = _DataSnapshot.GetFloat("bpm");
		m_Speed          = _DataSnapshot.GetFloat("speed");
		m_DefaultPayout  = _DataSnapshot.GetLong("default_payout");
		m_BronzePayout   = _DataSnapshot.GetLong("bronze_payout");
		m_SilverPayout   = _DataSnapshot.GetLong("silver_payout");
		m_GoldPayout     = _DataSnapshot.GetLong("gold_payout");
		m_PlatinumPayout = _DataSnapshot.GetLong("platinum_payout");
		m_Skin           = _DataSnapshot.GetString("skin", "default");
		
		LoadThumbnail();
		
		LoadPreview();
		
		LoadSkinAsset();
	}

	public override Dictionary<string, object> Serialize()
	{
		if (m_Remove)
			return null;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]          = m_Active;
		data["title"]           = m_Title;
		data["artist"]          = m_Artist;
		data["mode"]            = (int)m_Mode;
		data["level"]           = m_Level;
		data["price"]           = m_Price;
		data["default_payout"]  = m_DefaultPayout;
		data["bronze_payout"]   = m_BronzePayout;
		data["gold_payout"]     = m_GoldPayout;
		data["silver_payout"]   = m_SilverPayout;
		data["platinum_payout"] = m_PlatinumPayout;
		data["length"]          = m_Length;
		data["bpm"]             = m_BPM;
		data["speed"]           = m_Speed;
		data["skin"]            = m_SkinAsset != null ? m_SkinAsset.name : "default";
		
		return data;
	}
}

public class LevelDatabaseEditorWindow : DatabaseEditorWindow<LevelDatabaseEntry>
{
	[MenuItem("Database/Levels...")]
	public static void Open()
	{
		LevelDatabaseEditorWindow window = GetWindow<LevelDatabaseEditorWindow>(true, "Levels");
		window.minSize = new Vector2(640, 480);
		window.ShowUtility();
	}

	protected override DatabaseReference DatabaseReference => FirebaseDatabase.DefaultInstance.RootReference.Child("levels");

	protected override List<LevelDatabaseEntry> Deserialize(DataSnapshot _DataSnapshot)
	{
		List<LevelDatabaseEntry> levels = new List<LevelDatabaseEntry>();
		
		foreach (DataSnapshot levelSnapshot in _DataSnapshot.Children)
		{
			LevelDatabaseEntry level = new LevelDatabaseEntry();
			level.Deserialize(levelSnapshot);
			levels.Add(level);
		}
		
		return levels;
	}

	protected override string Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (LevelDatabaseEntry entry in Entries)
			data[entry.Key] = entry.Serialize();
		
		return Json.Serialize(data);
	}
}