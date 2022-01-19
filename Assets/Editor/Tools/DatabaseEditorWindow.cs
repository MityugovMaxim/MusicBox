using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Storage;
using UnityEditor;
using UnityEngine;

public abstract class DatabaseEntry
{
	public abstract string Key { get; }

	public abstract void Draw();

	public abstract void Deserialize(DataSnapshot _DataSnapshot);

	public abstract Dictionary<string, object> Serialize();

	protected async Task<Texture2D> LoadTexture(string _Path)
	{
		await FirebaseAdmin.Login();
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_Path);
		
		Texture2D texture;
		
		try
		{
			Uri uri = await reference.GetDownloadUrlAsync();
			
			texture      = await WebRequest.LoadTexture(uri.ToString());
			texture.name = Path.GetFileNameWithoutExtension(_Path);
		}
		catch
		{
			texture = null;
		}
		
		return texture;
	}

	protected async Task UploadTexture(string _Path, Texture2D _Texture)
	{
		string path = AssetDatabase.GetAssetPath(_Texture);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		await FirebaseAdmin.Login();
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_Path);
		
		try
		{
			await reference.PutFileAsync(path);
		}
		catch
		{
			Debug.LogErrorFormat("[DatabaseEntry] Upload texture failed. Path: {0}", _Path);
		}
	}
}

public abstract class DatabaseEditorWindow<T> : EditorWindow where T : DatabaseEntry
{
	protected IReadOnlyList<T> Entries => m_Entries;

	[SerializeField] List<T> m_Entries = new List<T>();

	protected abstract DatabaseReference DatabaseReference { get; }

	[SerializeField] Vector2 m_ScrollPosition;
	[SerializeField] Color   m_SeparatorColor = new Color(0.12f, 0.12f, 0.12f);

	bool m_Locked;

	void OnGUI()
	{
		EditorGUI.BeginDisabledGroup(m_Locked);
		
		EditorGUILayout.BeginHorizontal();
		
		if (GUILayout.Button("FETCH"))
			Fetch();
		
		if (GUILayout.Button("UPLOAD"))
			Upload();
		
		EditorGUILayout.EndHorizontal();
		
		m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
		
		for (int i = 0; i < Entries.Count; i++)
		{
			Entries[i].Draw();
			
			if (i >= Entries.Count - 1)
				continue;
			
			EditorGUILayout.Separator();
			Rect rect = GUILayoutUtility.GetLastRect();
			EditorGUI.DrawRect(rect, m_SeparatorColor);
		}
		
		if (GUILayout.Button("CREATE"))
			m_Entries.Add(Activator.CreateInstance<T>());
		
		EditorGUILayout.EndScrollView();
		
		EditorGUI.BeginDisabledGroup(m_Locked);
		
		if (m_Locked)
			EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0, 0, 0, 0.75f));
	}

	async void Fetch()
	{
		m_Locked = true;
		
		Repaint();
		
		await FirebaseAdmin.Login();
		
		DataSnapshot dataSnapshot = await DatabaseReference.GetValueAsync();
		
		m_Entries.Clear();
		m_Entries.AddRange(Deserialize(dataSnapshot));
		
		m_Locked = false;
		
		Repaint();
	}

	async void Upload()
	{
		m_Locked = true;
		
		Repaint();
		
		await FirebaseAdmin.Login();
		
		string data = Serialize();
		
		await DatabaseReference.SetRawJsonValueAsync(data);
		
		m_Locked = false;
		
		Repaint();
	}

	protected abstract List<T> Deserialize(DataSnapshot _DataSnapshot);

	protected abstract string Serialize();
}