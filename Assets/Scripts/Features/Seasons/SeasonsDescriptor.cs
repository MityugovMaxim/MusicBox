using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class SeasonsDescriptor : DescriptorsCollection, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Name => "seasons_descriptors";

	protected override Task OnLoad()
	{
		Log.Info(this, "Seasons descriptors loaded.");
		
		return base.OnLoad();
	}
}
