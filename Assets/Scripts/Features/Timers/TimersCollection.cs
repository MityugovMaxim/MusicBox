using UnityEngine.Scripting;

[Preserve]
public class TimersCollection : ProfileCollection<TimerSnapshot>, IDataCollection
{
	public DataCollectionPriority Priority => DataCollectionPriority.Low;

	protected override string Name => "timers";
}
