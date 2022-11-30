using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class DailyCollection : DataCollection<DailySnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "daily";

	protected override Task OnLoad()
	{
		Log.Info(this, "Daily loaded.");
		
		return base.OnLoad();
	}
}
