using System.Threading.Tasks;
using AudioBox.Logging;

public class ProfileSongs : ProfileCollection<DataSnapshot<long>>, IDataCollection
{
	public DataPriority Priority => DataPriority.High;

	protected override string Name => "songs";

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile songs loaded.");
		
		return base.OnLoad();
	}
}
