using System.Threading.Tasks;
using AudioBox.Logging;

public class ProfileVouchers : ProfileCollection<ProfileVoucher>
{
	protected override string Name => "vouchers";

	protected override Task OnLoad()
	{
		Log.Info(this, "Profile vouchers loaded.");
		
		return base.OnLoad();
	}
}