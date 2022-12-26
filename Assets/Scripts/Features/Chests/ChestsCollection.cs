using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class ChestsCollection : DataCollection<ChestSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "chests";

	protected override Task OnLoad()
	{
		Log.Info(this, "Chests loaded.");
		
		return base.OnLoad();
	}
}
