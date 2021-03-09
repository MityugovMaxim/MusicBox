using UnityEngine;

#if UNITY_EDITOR
public partial class EventTrack
{
	protected override float MinHeight => 30;
	protected override float MaxHeight => 30;
}
#endif

[CreateAssetMenu(fileName = "Event Track", menuName = "Tracks/Event Track")]
public partial class EventTrack : Track<EventClip>
{
	[SerializeField, Reference(typeof(Component))] string m_ComponentReference;
	[SerializeField]                               string m_MethodName;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		Component component = GetReference<Component>(m_ComponentReference);
		
		foreach (EventClip clip in Clips)
			clip.Initialize(component, m_MethodName);
	}
}