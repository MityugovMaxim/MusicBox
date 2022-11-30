using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;

namespace Features.Offers
{
	[Preserve]
	public class ProfileOffers : ProfileCollection<ProfileOffer>
	{
		protected override string Name => "offers";

		protected override Task OnLoad()
		{
			Log.Info(this, "Profile offers loaded.");
		
			return base.OnLoad();
		}
	}
}