using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
public partial class EventTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
[CreateAssetMenu(fileName = "Event Track", menuName = "Tracks/Event Track")]
public partial class EventTrack : Track<EventClip>
{
	[SerializeField, Reference(typeof(GameObject))] string m_Target;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		GameObject target = GetReference(m_Target);
		
		if (target == null)
		{
			Debug.LogError($"[EventTrack] Track '{name}' target is not assigned.");
			return;
		}
		
		IEventClipReceiver[] receivers = target.GetComponents<MonoBehaviour>()
			.OfType<IEventClipReceiver>()
			.ToArray();
		
		if (receivers.Length == 0)
			Debug.LogWarning($"[EventTrack] There are no receivers for track '{this}'", this);
		
		foreach (EventClip clip in Clips)
			clip.Initialize(Sequencer, receivers);
	}
}