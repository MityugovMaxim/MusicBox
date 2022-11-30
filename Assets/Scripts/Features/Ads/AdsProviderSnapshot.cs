using System.Collections.Generic;
using Firebase.Database;
using JetBrains.Annotations;
using UnityEngine.Scripting;

[Preserve]
public class AdsProviderSnapshot : Snapshot
{
	public bool Active { get; }

	[UsedImplicitly]
	public string iOSInterstitialID { get; }

	[UsedImplicitly]
	public string iOSRewardedID { get; }

	[UsedImplicitly]
	public string AndroidInterstitialID { get; }

	[UsedImplicitly]
	public string AndroidRewardedID { get; }

	public AdsProviderSnapshot() : base("new_ads_provider", 0)
	{
		Active                = false;
		iOSInterstitialID     = string.Empty;
		iOSRewardedID         = string.Empty;
		AndroidInterstitialID = string.Empty;
		AndroidRewardedID     = string.Empty;
	}

	public AdsProviderSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active                = _Data.GetBool("active");
		iOSInterstitialID     = _Data.GetString("ios_interstitial");
		iOSRewardedID         = _Data.GetString("ios_rewarded");
		AndroidInterstitialID = _Data.GetString("android_interstitial");
		AndroidRewardedID     = _Data.GetString("android_rewarded");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]               = Active;
		_Data["ios_interstitial"]     = iOSInterstitialID;
		_Data["ios_rewarded"]         = iOSRewardedID;
		_Data["android_interstitial"] = AndroidInterstitialID;
		_Data["android_rewarded"]     = AndroidInterstitialID;
	}
}