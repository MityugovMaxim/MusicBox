using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

public partial class MusicTrack
{
	protected override float MinHeight => 50;

	public override void DropPerform(float _Time, Object[] _Objects)
	{
		base.DropPerform(_Time, _Objects);
		
		AudioSource audioSource = GetReference<AudioSource>(m_AudioSource);
		
		if (audioSource == null)
			audioSource = AddReference<AudioSource>();
		
		AudioClip[] audioClips = _Objects.OfType<AudioClip>().ToArray();
		
		foreach (AudioClip audioClip in audioClips)
		{
			MusicClip musicClip = CreateInstance<MusicClip>();
			
			musicClip.name = "Music Clip";
			
			using (SerializedObject musicClipObject = new SerializedObject(musicClip))
			{
				SerializedProperty audioClipProperty = musicClipObject.FindProperty("m_AudioClip");
				
				audioClipProperty.objectReferenceValue = audioClip;
				
				musicClipObject.ApplyModifiedProperties();
			}
			
			TrackUtility.AddClip(this, musicClip, _Time, audioClip.length);
			
			_Time += audioClip.length;
			
			musicClip.Initialize(Sequencer, audioSource);
		}
	}
}
#endif

[CreateAssetMenu(fileName = "Music Track", menuName = "Tracks/Music Track")]
public partial class MusicTrack : Track<MusicClip>
{
	protected override float Offset => 0;

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