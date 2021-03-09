using UnityEngine;

#if UNITY_EDITOR
public partial class EventTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Event Track", menuName = "Tracks/Event Track")]
public partial class EventTrack : Track<EventClip>
{
	[SerializeField, Reference(typeof(GameObject))] string m_Component;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		GameObject gameObject = GetReference(m_Component);
		
		foreach (EventClip clip in Clips)
			clip.Initialize(gameObject);
	}
}