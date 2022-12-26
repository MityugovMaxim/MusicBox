using Firebase.Database;
using JetBrains.Annotations;
using UnityEngine.Scripting;

[Preserve]
public class AdsProviderSnapshot : Snapshot
{
	public bool Active { get; }

	[UsedImplicitly]
	public string AndroidInterstitialID { get; }

	[UsedImplicitly]
	public string AndroidRewardedID { get; }

	public AdsProviderSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active                = _Data.GetBool("active");
		AndroidInterstitialID = _Data.GetString("android_interstitial");
		AndroidRewardedID     = _Data.GetString("android_rewarded");
	}
}
