using System.Threading.Tasks;
using AudioBox.Logging;

public class FramesCollection : DataCollection<FrameSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "frames";

	protected override Task OnLoad()
	{
		Log.Info(this, "Frames loaded.");
		
		return base.OnLoad();
	}
}
