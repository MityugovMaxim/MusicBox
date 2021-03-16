using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
public partial class RoutineTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Routine Track", menuName = "Tracks/Routine Track")]
public partial class RoutineTrack : Track<RoutineClip>
{
	[SerializeField, Reference(typeof(GameObject))] string m_Target;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		GameObject target = GetReference(m_Target);
		
		if (target == null)
		{
			Debug.LogError($"[RoutineTrack] Track '{name}' target is not assigned.", this);
			return;
		}
		
		IRoutineClipReceiver[] receivers = target.GetComponents<MonoBehaviour>()
			.OfType<IRoutineClipReceiver>()
			.ToArray();
		
		if (receivers.Length == 0)
			Debug.LogWarning($"[RoutineTrack] There are no receivers for track '{name}'", this);
		
		foreach (RoutineClip clip in Clips)
			clip.Initialize(Sequencer, receivers);
	}
}