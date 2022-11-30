using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class NewsDescriptor : DescriptorsCollection, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Name => "news_descriptors";

	protected override Task OnLoad()
	{
		Log.Info(this, "News descriptors loaded.");
		
		return base.OnLoad();
	}
}
