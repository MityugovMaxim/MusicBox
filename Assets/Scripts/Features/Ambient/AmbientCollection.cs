using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class AmbientCollection : DataCollection<AmbientSnapshot>, IDataCollection
{
	public DataCollectionPriority Priority => DataCollectionPriority.Low;

	protected override string Path => "ambient";

	protected override Task OnLoad()
	{
		Log.Info(this, "Ambient loaded.");
		
		return base.OnLoad();
	}
}
