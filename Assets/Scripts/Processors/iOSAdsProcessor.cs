using UnityEngine.Scripting;
#if UNITY_IOS
[Preserve]
public class iOSAdsProcessor : AdsProcessor
{
	protected override string GameID         => "4234912";
	protected override string InterstitialID => "Interstitial_iOS";
	protected override string RewardedID     => "Rewarded_iOS";
}
#endif