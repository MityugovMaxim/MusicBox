using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class ProfileTimers : ProfileCollection<TimerSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Name => "timers";

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile timers loaded.");
		
		return base.OnLoad();
	}
}
