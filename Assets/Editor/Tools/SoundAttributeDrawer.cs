using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SoundAttribute))]
public class SoundAttributeDrawer : PropertyDrawer
{
	public static bool Loaded { get; set; }

	static SoundInfo[]  m_SoundInfos;
	static string[]     m_Sounds;
	static GUIContent[] m_Content;

	static MethodInfo m_PlayMethodInfo;
	static MethodInfo m_StopMethodInfo;

	public override void OnGUI(Rect _Rect, SerializedProperty _Property, GUIContent _Label)
	{
		if (_Property.propertyType != SerializedPropertyType.String)
		{
			EditorGUI.PropertyField(_Rect, _Property, _Label);
			return;
		}
		
		EditorGUI.BeginChangeCheck();
		
		LoadSounds();
		
		int index = Array.FindIndex(m_Sounds, _SoundID => _SoundID == _Property.stringValue);
		
		const float buttonSize = 18;
		
		Rect popupRect = new Rect(
			_Rect.x,
			_Rect.y + (_Rect.height - EditorGUIUtility.singleLineHeight) * 0.5f,
			_Rect.width - buttonSize * 2 - 6,
			EditorGUIUtility.singleLineHeight
		);
		
		Rect playRect = new Rect(
			_Rect.xMax - buttonSize * 2 - 3,
			_Rect.y + (_Rect.height - buttonSize) * 0.5f,
			buttonSize,
			buttonSize
		);
		
		Rect stopRect = new Rect(
			_Rect.xMax - buttonSize,
			_Rect.y + (_Rect.height - buttonSize) * 0.5f,
			buttonSize,
			buttonSize
		);
		
		EditorGUI.BeginProperty(_Rect, _Label, _Property);
		index = EditorGUI.Popup(popupRect, _Label, index, m_Content);
		EditorGUI.EndProperty();
		
		if (GUI.Button(playRect, "▶") && index >= 0 && index < m_Sounds.Length)
			Play(m_Sounds[index]);
		if (GUI.Button(stopRect, "■"))
			StopAudioClip();
		
		if (EditorGUI.EndChangeCheck())
		{
			_Property.stringValue = m_Sounds[Mathf.Clamp(index, 0, m_Sounds.Length)];
			_Property.serializedObject.ApplyModifiedProperties();
		}
	}

	static void Play(string _SoundID)
	{
		if (!Loaded || m_SoundInfos == null || m_SoundInfos.Length == 0)
			return;
		
		SoundInfo soundInfo = m_SoundInfos.FirstOrDefault(_SoundInfo => _SoundInfo.ID == _SoundID);
		
		if (soundInfo == null)
			return;
		
		AudioClip audioClip = Resources.Load<AudioClip>(soundInfo.Path);
		
		if (audioClip == null)
			return;
		
		StopAudioClip();
		PlayAudioClip(audioClip);
	}

	static void PlayAudioClip(AudioClip _AudioClip)
	{
		object[] args = { _AudioClip, 0, false };
		
		if (m_PlayMethodInfo != null)
		{
			m_PlayMethodInfo.Invoke(null, args);
			return;
		}
		
		Assembly assembly = typeof(AudioImporter).Assembly;
		
		Type type = assembly.GetType("UnityEditor.AudioUtil");
		
		m_PlayMethodInfo = type.GetMethod(
			"PlayPreviewClip",
			BindingFlags.Static | BindingFlags.Public,
			null,
			new Type[]
			{
				typeof(AudioClip),
				typeof(int),
				typeof(bool)
			},
			null
		);
		
		m_PlayMethodInfo?.Invoke(null, args);
	}

	static void StopAudioClip()
	{
		if (m_StopMethodInfo != null)
		{
			m_StopMethodInfo.Invoke(null, null);
			return;
		}
		
		Assembly assembly = typeof(AudioImporter).Assembly;
		
		Type type = assembly.GetType("UnityEditor.AudioUtil");
		
		m_StopMethodInfo = type.GetMethod(
			"StopAllPreviewClips",
			BindingFlags.Static | BindingFlags.Public
		);
		
		m_StopMethodInfo?.Invoke(null, null);
	}

	static void LoadSounds()
	{
		if (Loaded && m_Sounds != null && m_Sounds.Length != 0)
			return;
		
		Loaded = true;
		
		m_SoundInfos = AssetDatabase.FindAssets($"t:{nameof(SoundRegistry)}")
			.Select(AssetDatabase.GUIDToAssetPath)
			.Select(AssetDatabase.LoadAssetAtPath<SoundRegistry>)
			.Where(_Registry => _Registry != null)
			.SelectMany(_Registry => _Registry.ToArray())
			.ToArray();
		
		m_Sounds = m_SoundInfos
			.Select(_SoundInfo => _SoundInfo.ID)
			.Distinct()
			.Prepend(string.Empty)
			.ToArray();
		
		m_Content = m_Sounds
			.Select(_SoundID => string.IsNullOrEmpty(_SoundID) ? new GUIContent("None") : new GUIContent(_SoundID))
			.ToArray();
	}
}
