using System.Threading.Tasks;
using AudioBox.Logging;

public class ProfileFrames : ProfileCollection<ProfileFrame>
{
	protected override string Name => "frames";

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile frames loaded.");
		
		return base.OnLoad();
	}
}