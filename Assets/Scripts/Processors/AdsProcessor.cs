using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public interface IAdsProvider
{
	string ID             { get; }
	Task<bool> Initialize(string _InterstitialID, string _RewardedID);
	Task<bool> Interstitial(string _Place);
	Task<bool> Rewarded(string _Place);
}

public enum AdsState
{
	Unavailable = 0,
	Completed   = 1,
	Canceled    = 2,
	Failed      = 3,
	Timeout     = 4,
}

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

[Preserve]
public class AdsProvidersDataUpdateSignal { }

[Preserve]
public class AdsProcessor : DataProcessor<AdsProviderSnapshot, AdsProvidersDataUpdateSignal>
{
	protected override string Path => "ads_providers";

	[Inject] IAdsProvider[]  m_AdsProviders;
	[Inject] AudioManager    m_AudioManager;
	[Inject] ConfigProcessor m_ConfigProcessor;

	float m_Time;

	protected override Task OnFetch()
	{
		InitializeAdsProviders();
		
		return Task.CompletedTask;
	}

	async void InitializeAdsProviders()
	{
		List<Task<bool>> tasks = new List<Task<bool>>();
		
		List<string> adsProviderIDs = GetAdsProviderIDs();
		
		foreach (string adsProviderID in adsProviderIDs)
		{
			IAdsProvider adsProvider = GetAdsProvider(adsProviderID);
			
			if (adsProvider == null)
				continue;
			
			string interstitialID = GetInterstitialID(adsProviderID);
			string rewardedID     = GetRewardedID(adsProviderID);
			
			tasks.Add(adsProvider.Initialize(interstitialID, rewardedID));
		}
		
		await Task.WhenAll(tasks);
		
		Log.Info(this, "Ads providers initialized.");
	}

	List<string> GetAdsProviderIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	string GetInterstitialID(string _AdsProviderID)
	{
		AdsProviderSnapshot snapshot = GetSnapshot(_AdsProviderID);
		
		#if UNITY_IOS
		return snapshot?.iOSInterstitialID ?? string.Empty;
		#elif UNITY_ANDROID
		return snapshot?.AndroidInterstitialID ?? string.Empty;
		#else
		return string.Empty;
		#endif
	}

	string GetRewardedID(string _AdsProviderID)
	{
		AdsProviderSnapshot snapshot = GetSnapshot(_AdsProviderID);
		
		#if UNITY_IOS
		return snapshot?.iOSRewardedID ?? string.Empty;
		#elif UNITY_ANDROID
		return snapshot?.AndroidRewardedID ?? string.Empty;
		#else
		return string.Empty;
		#endif
	}

	IAdsProvider GetAdsProvider(string _AdsProviderID)
	{
		if (string.IsNullOrEmpty(_AdsProviderID))
			return null;
		
		return m_AdsProviders.FirstOrDefault(_AdsProvider => _AdsProvider.ID == _AdsProviderID);
	}

	public bool CheckAvailable() => Time.realtimeSinceStartup >= m_Time;

	public bool CheckUnavailable() => Time.realtimeSinceStartup < m_Time;

	public async Task<bool> Interstitial(string _Place)
	{
		if (CheckUnavailable())
			return true;
		
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Interstitial(_Place))
				continue;
			
			ProcessCooldown();
			
			AudioListener.volume = 1;
			
			m_AudioManager.SetAudioActive(true);
			
			return true;
		}
		
		AudioListener.volume = 1;
		
		m_AudioManager.SetAudioActive(true);
		
		return false;
	}

	public async Task<bool> Rewarded(string _Place)
	{
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Rewarded(_Place))
				continue;
			
			ProcessCooldown();
			
			AudioListener.volume = 1;
			
			m_AudioManager.SetAudioActive(true);
			
			return true;
		}
		
		AudioListener.volume = 1;
		
		m_AudioManager.SetAudioActive(true);
		
		return false;
	}

	void ProcessCooldown()
	{
		m_Time = Time.realtimeSinceStartup + m_ConfigProcessor.AdsCooldown;
	}
}
