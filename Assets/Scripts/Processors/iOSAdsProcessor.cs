#if UNITY_IOS
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class iOSAdsProcessor : AdsProcessor
{
	protected override string GameID         => "4234912";
	protected override string InterstitialID => "Interstitial_iOS";
	protected override string RewardedID     => "Rewarded_iOS";

	[Inject]
	public iOSAdsProcessor(ProfileProcessor _ProfileProcessor) : base(_ProfileProcessor) { }
}
#endif