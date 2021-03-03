using UnityEngine;

#if UNITY_EDITOR
public partial class MusicTrack
{
	public override float MinHeight => 80;
}
#endif

[CreateAssetMenu(fileName = "Music Track", menuName = "Tracks/Music Track")]
public partial class MusicTrack : Track<MusicClip>
{
	[SerializeField, Reference(typeof(AudioSource))] string m_AudioSourceReference;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		AudioSource audioSource = GetReference<AudioSource>(m_AudioSourceReference);
		
		if (audioSource == null)
			audioSource = AddReference<AudioSource>();
		
		foreach (MusicClip clip in Clips)
			clip.Initialize(audioSource);
	}
}