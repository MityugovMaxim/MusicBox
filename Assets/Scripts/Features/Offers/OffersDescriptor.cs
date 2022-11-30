using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class OffersDescriptor : DescriptorsCollection, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Name => "offers_descriptors";

	protected override Task OnLoad()
	{
		Log.Info(this, "Offers loaded.");
		
		return base.OnLoad();
	}
}
