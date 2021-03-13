using UnityEditor;

public static class TrackUtility
{
	public static void AddClip(Track _Track, Clip _Clip)
	{
		AssetDatabase.AddObjectToAsset(_Clip, _Track);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
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
	}

	public static void AddClip(Track _Track, Clip _Clip, float _Time)
	{
		AssetDatabase.AddObjectToAsset(_Clip, _Track);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		float duration = _Clip.MaxTime - _Clip.MinTime;
		
		using (SerializedObject clipObject = new SerializedObject(_Clip))
		{
			SerializedProperty minTimeProperty = clipObject.FindProperty("m_MinTime");
			SerializedProperty maxTimeProperty = clipObject.FindProperty("m_MaxTime");
			
			minTimeProperty.floatValue = _Time;
			maxTimeProperty.floatValue = _Time + duration;
			
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
	}
}