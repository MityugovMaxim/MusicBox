using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

public partial class MusicTrack
{
	protected override float MinHeight => 50;

	public override void DragPerform(float _Time, Object[] _Objects)
	{
		base.DragPerform(_Time, _Objects);
		
		AudioClip[] audioClips = _Objects.OfType<AudioClip>().ToArray();
		
		foreach (AudioClip audioClip in audioClips)
		{
			MusicClip musicClip = CreateInstance<MusicClip>();
			
			musicClip.name = "Music Clip";
			
			using (SerializedObject musicClipObject = new SerializedObject(musicClip))
			{
				SerializedProperty audioClipProperty = musicClipObject.FindProperty("m_AudioClip");
				SerializedProperty minTimeProperty   = musicClipObject.FindProperty("m_MinTime");
				SerializedProperty maxTimeProperty   = musicClipObject.FindProperty("m_MaxTime");
				
				audioClipProperty.objectReferenceValue = audioClip;
				minTimeProperty.floatValue = _Time;
				maxTimeProperty.floatValue = _Time + audioClip.length;
				
				_Time += audioClip.length;
				
				musicClipObject.ApplyModifiedProperties();
			}
			
			Clips.Add(musicClip);
			
			AssetDatabase.AddObjectToAsset(musicClip, this);
		}
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
}
#endif

[CreateAssetMenu(fileName = "Music Track", menuName = "Tracks/Music Track")]
public partial class MusicTrack : Track<MusicClip>
{
	[SerializeField, Reference(typeof(AudioSource))] string m_AudioSource;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		AudioSource audioSource = GetReference<AudioSource>(m_AudioSource);
		
		if (audioSource == null)
			audioSource = AddReference<AudioSource>();
		
		foreach (MusicClip clip in Clips)
			clip.Initialize(Sequencer, audioSource);
	}
}