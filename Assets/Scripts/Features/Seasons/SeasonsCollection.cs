using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class SeasonsCollection : DataCollection<SeasonSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "season";

	protected override Task OnLoad()
	{
		Log.Info(this, "Seasons loaded.");
		
		return base.OnLoad();
	}
}
