using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace AudioBox.AudioWave
{
	[InitializeOnLoad]
	public class UIAudioWaveReload : ScriptableObject
	{
		[Preserve]
		static UIAudioWaveReload()
		{
			EditorApplication.delayCall += () => CreateInstance<UIAudioWaveReload>();
		}

		void Awake()
		{
			Undo.undoRedoPerformed -= ProcessUndoRedo;
			Undo.undoRedoPerformed += ProcessUndoRedo;
		}

		void OnEnable()
		{
			UIAudioWave[] audioWaves = GameObject.FindObjectsOfType<UIAudioWave>();
			foreach (UIAudioWave audioWave in audioWaves)
				audioWave.Render();
		}

		void OnDisable()
		{
			IDisposable[] audioWaves = GameObject.FindObjectsOfType<UIAudioWave>().Cast<IDisposable>().ToArray();
			foreach (IDisposable audioWave in audioWaves)
				audioWave.Dispose();
		}

		static void ProcessUndoRedo()
		{
			UIAudioWave[] audioWaves = GameObject.FindObjectsOfType<UIAudioWave>();
			foreach (UIAudioWave audioWave in audioWaves)
				audioWave.Render();
		}
	}

	[CanEditMultipleObjects]
	[CustomEditor(typeof(UIAudioWave))]
	public class UIAudioWaveEditor : Editor
	{
		static UIAudioWaveEditor() { }

		SerializedProperty AudioClipProperty => m_AudioClipProperty ?? (m_AudioClipProperty = serializedObject.FindProperty("m_AudioClip"));

		SerializedProperty DurationProperty => m_DurationProperty ?? (m_DurationProperty = serializedObject.FindProperty("m_Duration"));

		SerializedProperty m_AudioClipProperty;
		SerializedProperty m_DurationProperty;

		static readonly HashSet<string> m_ExcludedFields = new HashSet<string>()
		{
			"m_Material",
			"m_AudioClip",
			//"m_Duration",
			"m_OnCullStateChanged",
			"m_Sprite",
		};

		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			
			SerializedProperty property = serializedObject.GetIterator();
			
			property.NextVisible(true);
			
			using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
				EditorGUILayout.PropertyField(property, true);
			
			DrawAudioClip();
			
			//DrawDuration();
			
			while (property.NextVisible(false))
			{
				if (m_ExcludedFields.Contains(property.propertyPath))
					continue;
				
				EditorGUILayout.PropertyField(property, true);
			}
			
			serializedObject.ApplyModifiedProperties();
		}

		void DrawAudioClip()
		{
			EditorGUILayout.BeginHorizontal();
			
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.PropertyField(AudioClipProperty);
			
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				serializedObject.UpdateIfRequiredOrScript();
				Render();
			}
			
			if (GUILayout.Button("Render", GUILayout.Width(120)))
				Render();
			
			EditorGUILayout.EndHorizontal();
		}

		void DrawDuration()
		{
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.PropertyField(DurationProperty);
			
			if (EditorGUI.EndChangeCheck())
			{
				//Limit();
				//Render();
				serializedObject.ApplyModifiedProperties();
			}
		}

		void Render()
		{
			foreach (UIAudioWave audioWave in targets.OfType<UIAudioWave>())
				audioWave.Render();
		}

		void Limit()
		{
			double duration = DurationProperty.doubleValue;
			
			AudioClip audioClip = AudioClipProperty.objectReferenceValue as AudioClip;
			
			double limit = audioClip != null ? audioClip.length : 0;
			
			DurationProperty.doubleValue = Math.Clamp(duration, 0, limit);
			
			//DurationProperty.serializedObject.ApplyModifiedProperties();
		}
	}
}