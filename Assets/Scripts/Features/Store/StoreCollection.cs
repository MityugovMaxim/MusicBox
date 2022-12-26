using System.Threading.Tasks;
using AudioBox.Logging;

public class StoreCollection : DataCollection<StoreSnapshot>, IDataCollection
{
	protected override string Path => "store";

	public DataPriority Priority => DataPriority.Low;

	protected override Task OnLoad()
	{
		Log.Info(this, "Store loaded.");
		
		return base.OnLoad();
	}
}