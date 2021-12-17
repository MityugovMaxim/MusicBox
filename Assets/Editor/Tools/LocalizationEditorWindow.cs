using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Purchasing;

public class LocalizationEditorWindow : EditorWindow
{
	[MenuItem("Tools/Localization Editor...")]
	public static void Open()
	{
		LocalizationEditorWindow window = GetWindow<LocalizationEditorWindow>(true);
		window.titleContent = new GUIContent("Localization Editor");
		window.ShowUtility();
	}

	class Localization
	{
		public string Key   { get; set; }
		public string Value { get; set; }

		public Localization(string _Key, string _Value)
		{
			Key   = _Key;
			Value = _Value;
		}
	}

	static readonly SystemLanguage[] m_Languages =
	{
		SystemLanguage.English,
		SystemLanguage.Russian,
		SystemLanguage.German,
		SystemLanguage.Spanish,
		SystemLanguage.Portuguese,
	};

	static GUIContent[] LanguagesContent
	{
		get
		{
			if (m_LanguagesContent == null)
				m_LanguagesContent = m_Languages.Select(_Language => new GUIContent(_Language.ToString())).ToArray();
			return m_LanguagesContent;
		}
	}

	static GUIContent[] m_LanguagesContent;

	readonly HashSet<string>                                m_Keys          = new HashSet<string>();
	readonly Dictionary<SystemLanguage, List<Localization>> m_Localizations = new Dictionary<SystemLanguage, List<Localization>>();

	ReorderableList m_LocalizationsList;

	int     m_LanguageIndex;
	Vector2 m_ScrollPosition;
	string  m_Key;
	string  m_Value;

	void OnEnable()
	{
		m_Localizations.Clear();
		foreach (SystemLanguage language in m_Languages)
			m_Localizations[language] = new List<Localization>();
	}

	void OnGUI()
	{
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("FETCH"))
			Fetch();
		
		if (GUILayout.Button("UPLOAD"))
			Upload();
		
		GUILayout.EndHorizontal();
		
		EditorGUI.BeginChangeCheck();
		
		m_LanguageIndex = EditorGUILayout.Popup(new GUIContent("Language"), m_LanguageIndex, LanguagesContent);
		
		if (EditorGUI.EndChangeCheck())
			CreateLocalizationsList();
		
		DrawLocalization();
	}

	async void Fetch()
	{
		m_Localizations.Clear();
		
		Debug.Log("[LocalizationEditorWindow] Fetching localizations...");
		
		foreach (SystemLanguage language in m_Languages)
			await Fetch(language);
		
		m_Keys.Clear();
		foreach (Localization localization in m_Localizations.SelectMany(_Entry => _Entry.Value))
			m_Keys.Add(localization.Key);
		
		foreach (string key in m_Keys)
		foreach (List<Localization> localizations in m_Localizations.Values.ToArray())
		{
			if (localizations.All(_Localization => _Localization.Key != key))
				localizations.Add(new Localization(key, "MISSING_VALUE"));
		}
		
		Debug.Log("[LocalizationEditorWindow] Fetch localizations completed.");
		
		CreateLocalizationsList();
		
		Repaint();
	}

	async void Upload()
	{
		Debug.Log("[LocalizationEditorWindow] Uploading localizations...");
		
		foreach (SystemLanguage language in m_Languages)
			await Upload(language);
		
		Debug.Log("[LocalizationEditorWindow] Upload localizations completed.");
		
		Repaint();
	}

	async Task Fetch(SystemLanguage _Language)
	{
		List<Localization> localizations = new List<Localization>();
		
		m_Localizations[_Language] = localizations;
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child($"Localization/{_Language}.json");
		
		try
		{
			byte[] file = await reference.GetBytesAsync(4 * 1024 * 1024);
			
			Dictionary<string, object> data = MiniJson.JsonDecode(Encoding.UTF8.GetString(file)) as Dictionary<string, object>;
			
			if (data == null)
				return;
			
			localizations.AddRange(data.Select(entry => new Localization(entry.Key, entry.Value.ToString())));
		}
		catch (Exception)
		{
			Debug.LogWarningFormat("[LocalizationEditorWindow] Fetch localization failed. Language: {0}.", _Language);
		}
	}

	async Task Upload(SystemLanguage _Language)
	{
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child($"Localization/{_Language}.json");
		
		List<Localization> localizations = m_Localizations[_Language];
		
		StringBuilder data = new StringBuilder();
		data.AppendLine("{");
		for (int i = 0; i < localizations.Count; i++)
		{
			data.Append($"\t\"{localizations[i].Key}\": \"{localizations[i].Value}\"");
			if (i < localizations.Count - 1)
				data.AppendLine(",");
			else
				data.AppendLine();
		}
		data.AppendLine("}");
		
		byte[] file = Encoding.UTF8.GetBytes(data.ToString());
		
		try
		{
			await reference.PutBytesAsync(file);
		}
		catch (Exception)
		{
			Debug.LogWarningFormat("[LocalizationEditorWindow] Upload localization failed. Language: {0}.", _Language);
		}
	}

	void CreateLocalizationsList()
	{
		if (m_LanguageIndex < 0 || m_LanguageIndex >= m_Languages.Length)
			return;
		
		SystemLanguage language = m_Languages[m_LanguageIndex];
		
		if (!m_Localizations.ContainsKey(language))
			return;
		
		List<Localization> localizations = m_Localizations[language];
		
		m_LocalizationsList = new ReorderableList(localizations, typeof(Localization), true, true, false, true);
		
		m_LocalizationsList.drawHeaderCallback += _Rect =>
		{
			EditorGUI.SelectableLabel(_Rect, "Localization Keys");
		};
		
		m_LocalizationsList.drawElementCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			Rect keyRect = new Rect(
				_Rect.x,
				_Rect.y,
				_Rect.width * 0.5f - 10,
				_Rect.height
			);
			
			Rect separatorRect = new Rect(
				_Rect.x + _Rect.width * 0.5f - 10,
				_Rect.y,
				20,
				_Rect.height
			);
			
			Rect valueRect = new Rect(
				_Rect.x + _Rect.width * 0.5f + 10,
				_Rect.y,
				_Rect.width * 0.5f - 10,
				_Rect.height
			);
			
			Localization localization = localizations[_Index];
			
			localization.Key   = EditorGUI.TextField(keyRect, localization.Key);
			localization.Value = EditorGUI.TextField(valueRect, localization.Value);
			
			EditorGUI.LabelField(separatorRect, ":");
		};
	}

	void DrawLocalization()
	{
		m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
		
		if (m_LocalizationsList == null)
			CreateLocalizationsList();
		else
			m_LocalizationsList.DoLayoutList();
		
		EditorGUILayout.EndScrollView();
		
		EditorGUILayout.BeginHorizontal();
		
		m_Key   = EditorGUILayout.TextField(m_Key);
		m_Value = EditorGUILayout.TextField(m_Value);
		
		if (m_LanguageIndex < 0 || m_LanguageIndex >= m_Languages.Length)
			return;
		
		SystemLanguage language = m_Languages[m_LanguageIndex];
		
		if (!m_Localizations.ContainsKey(language))
			return;
		
		List<Localization> localizations = m_Localizations[language];
		
		GUI.enabled = localizations.All(_Localization => _Localization.Key != m_Key);
		if (GUILayout.Button("ADD"))
		{
			localizations.Add(new Localization(m_Key, m_Value));
			
			foreach (var entry in m_Localizations)
			{
				if (entry.Key != language && entry.Value != null && entry.Value.All(_Localization => _Localization.Key != m_Key))
					entry.Value.Add(new Localization(m_Key, "MISSING_VALUE"));
			}
			
			m_Key   = string.Empty;
			m_Value = string.Empty;
			
			Repaint();
		}
		GUI.enabled = true;
		
		EditorGUILayout.EndHorizontal();
	}
}
