using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AdsProviderMadPixel : IAdsProvider
{
	string IAdsProvider.ID => "mad_pixel";

	[Inject] StatisticProcessor m_StatisticProcessor;

	bool   m_Initialized;
	string m_InterstitialID;
	string m_RewardedID;

	async Task<bool> IAdsProvider.Initialize(string _InterstitialID, string _RewardedID)
	{
		m_InterstitialID = _InterstitialID;
		m_RewardedID     = _RewardedID;
		
		bool interstitial = await MediationManager.Instance.WaitInterstitial();
		
		if (interstitial)
			Log.Info(this, "Interstitial load complete.");
		else
			Log.Error(this, "Interstitial load failed.");
		
		bool rewarded = await MediationManager.Instance.WaitRewarded();
		
		if (rewarded)
			Log.Info(this, "Rewarded load complete.");
		else
			Log.Error(this, "Rewarded load failed.");
		
		return interstitial && rewarded;
	}

	async Task<bool> IAdsProvider.Interstitial(string _Place)
	{
		const string type = "interstitial";
		
		string place = string.IsNullOrEmpty(_Place) ? m_InterstitialID : _Place;
		
		if (string.IsNullOrEmpty(place))
			return false;
		
		bool available = await MediationManager.Instance.WaitInterstitial();
		
		m_StatisticProcessor.LogAdsAvailable(type, place, available);
		
		if (!available)
			return false;
		
		m_StatisticProcessor.LogAdsStarted(type, place);
		
		AdsState state = await MediationManager.Instance.ShowInterstitialAsync();
		
		m_StatisticProcessor.LogAdsFinished(type, place, GetState(state));
		
		return state == AdsState.Completed || state == AdsState.Canceled;
	}

	async Task<bool> IAdsProvider.Rewarded(string _Place)
	{
		const string type = "rewarded";
		
		string place = string.IsNullOrEmpty(_Place) ? m_RewardedID : _Place;
		
		if (string.IsNullOrEmpty(place))
			return false;
		
		bool available = await MediationManager.Instance.WaitRewarded();
		
		m_StatisticProcessor.LogAdsAvailable(type, place, available);
		
		if (!available)
			return false;
		
		m_StatisticProcessor.LogAdsStarted(type, place);
		
		AdsState state = await MediationManager.Instance.ShowRewardedAsync();
		
		m_StatisticProcessor.LogAdsFinished(type, place, GetState(state));
		
		return state == AdsState.Completed;
	}

	static string GetState(AdsState _State)
	{
		switch (_State)
		{
			case AdsState.Unavailable: return "unavailable";
			case AdsState.Completed:   return "watched";
			case AdsState.Canceled:    return "canceled";
			case AdsState.Failed:      return "failed";
			case AdsState.Timeout:     return "unavailable";
			default:                   return "unavailable";
		}
	}
}