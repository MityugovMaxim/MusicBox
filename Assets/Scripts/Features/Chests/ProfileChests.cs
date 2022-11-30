using System.Threading.Tasks;
using AudioBox.Logging;

public class ProfileChests : ProfileCollection<ProfileChest>
{
	protected override string Name => "chests";

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile chests loaded.");
	
		return base.OnLoad();
	}
}
