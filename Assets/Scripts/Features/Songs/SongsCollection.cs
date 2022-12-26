using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class SongsCollection : DataCollection<SongSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.High;

	protected override string Path => "songs";

	protected override Task OnLoad()
	{
		Log.Info(this, "Slots loaded.");
		
		return base.OnLoad();
	}
}
