using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

[Preserve]
public class ProfileSeasons : ProfileCollection<ProfileSeason>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Name => "seasons";

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile seasons loaded.");
		
		return base.OnLoad();
	}
}
