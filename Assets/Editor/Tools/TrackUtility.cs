using UnityEditor;
using UnityEngine;

public static class TrackUtility
{
	public static void AddClip(Track _Track, Clip _Clip)
	{
		float time = _Clip.MinTime;
		
		AddClip(_Track, _Clip, time);
	}

	public static void AddClip(Track _Track, Clip _Clip, float _Time)
	{
		float duration = _Clip.MaxTime - _Clip.MinTime;
		
		AddClip(_Track, _Clip, _Time, duration);
	}

	public static void AddClip(Track _Track, Clip _Clip, float _Time, float _Duration)
	{
		_Clip.hideFlags = HideFlags.HideInHierarchy;
		
		AssetDatabase.AddObjectToAsset(_Clip, _Track);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		using (SerializedObject clipObject = new SerializedObject(_Clip))
		{
			SerializedProperty minTimeProperty = clipObject.FindProperty("m_MinTime");
			SerializedProperty maxTimeProperty = clipObject.FindProperty("m_MaxTime");
			
			minTimeProperty.floatValue = _Time;
			maxTimeProperty.floatValue = _Time + _Duration;
			
			clipObject.ApplyModifiedProperties();
		}
		
		using (SerializedObject trackObject = new SerializedObject(_Track))
		{
			SerializedProperty clipsProperty = trackObject.FindProperty("m_Clips");
			
			int index = clipsProperty.arraySize;
			
			clipsProperty.InsertArrayElementAtIndex(index);
			
			trackObject.ApplyModifiedProperties();
			trackObject.UpdateIfRequiredOrScript();
			
			SerializedProperty clipProperty = clipsProperty.GetArrayElementAtIndex(index);
			
			clipProperty.objectReferenceValue = _Clip;
			
			trackObject.ApplyModifiedProperties();
		}
		
		_Track.Sort();
	}

	public static void RemoveClip(Track _Track, Clip _Clip)
	{
		using (SerializedObject trackObject = new SerializedObject(_Track))
		{
			SerializedProperty clipsProperty = trackObject.FindProperty("m_Clips");
			
			for (int i = 0; i < clipsProperty.arraySize; i++)
			{
				SerializedProperty clipProperty = clipsProperty.GetArrayElementAtIndex(i);
				
				Clip clip = clipProperty.objectReferenceValue as Clip;
				
				if (_Clip != clip)
					continue;
				
				clipProperty.objectReferenceValue = null;
				
				clipsProperty.DeleteArrayElementAtIndex(i);
			}
			
			trackObject.ApplyModifiedProperties();
		}
		
		AssetDatabase.RemoveObjectFromAsset(_Clip);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		_Track.Sort();
	}
}