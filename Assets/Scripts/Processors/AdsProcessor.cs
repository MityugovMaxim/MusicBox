using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using Firebase.Extensions;
using JetBrains.Annotations;
using MAXHelper;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public interface IAdsProvider
{
	string ID             { get; }
	string InterstitialID { get; }
	string RewardedID     { get; }
	Task<bool> Initialize(string _InterstitialID, string _RewardedID);
	Task<bool> Interstitial();
	Task<bool> Interstitial(string _PlacementID);
	Task<bool> Rewarded();
	Task<bool> Rewarded(string _PlacementID);
}

[Preserve]
public class AdsProviderMadPixel : IAdsProvider
{
	string IAdsProvider.ID             => "mad_pixel";
	string IAdsProvider.InterstitialID => m_InterstitialID;
	string IAdsProvider.RewardedID     => m_RewardedID;

	[Inject] StatisticProcessor m_StatisticProcessor;

	bool   m_Initialized;
	string m_InterstitialID;
	string m_RewardedID;

	public async Task<bool> Initialize(string _InterstitialID, string _RewardedID)
	{
		m_InterstitialID = _InterstitialID;
		m_RewardedID     = _RewardedID;
		
		await UnityTask.Until(MaxSdk.IsInitialized);
		
		AdsManager.Instance.InitializeInterstitial(_InterstitialID);
		AdsManager.Instance.InitializeRewarded(_RewardedID);
		
		return true;
	}

	public Task<bool> Interstitial()
	{
		return Interstitial(m_InterstitialID);
	}

	public Task<bool> Interstitial(string _PlacementID)
	{
		const string type = "interstitial";
		
		if (string.IsNullOrEmpty(_PlacementID))
			return Task.FromResult(false);
		
		bool available = AdsManager.Instance.HasLoadedAd(false);
		
		m_StatisticProcessor.LogAdsAvailable(type, _PlacementID, available);
		
		if (!available)
			return Task.FromResult(false);
		
		m_StatisticProcessor.LogAdsStarted(type, _PlacementID);
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		AdsManager.Instance.ShowAd(
			_Success =>
			{
				m_StatisticProcessor.LogAdsFinished(type, _PlacementID, _Success ? "watched" : "canceled");
				
				completionSource.TrySetResult(_Success);
			},
			() =>
			{
				m_StatisticProcessor.LogAdsFinished(type, _PlacementID, "watched");
				
				completionSource.TrySetResult(true);
			},
			_PlacementID,
			false
		);
		
		return completionSource.Task;
	}

	public Task<bool> Rewarded()
	{
		return Rewarded(m_RewardedID);
	}

	public Task<bool> Rewarded(string _PlacementID)
	{
		const string type = "rewarded";
		
		if (string.IsNullOrEmpty(_PlacementID))
			return Task.FromResult(false);
		
		bool available = AdsManager.Instance.HasLoadedAd();
		
		m_StatisticProcessor.LogAdsAvailable(type, _PlacementID, available);
		
		if (!available)
			return Task.FromResult(false);
		
		m_StatisticProcessor.LogAdsStarted(type, _PlacementID);
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		AdsManager.Instance.ShowAd(
			_Success =>
			{
				m_StatisticProcessor.LogAdsFinished(type, _PlacementID, _Success ? "watched" : "canceled");
				
				completionSource.TrySetResult(_Success);
			},
			() =>
			{
				m_StatisticProcessor.LogAdsFinished(type, _PlacementID, "watched");
				
				completionSource.TrySetResult(false);
			},
			_PlacementID
		);
		
		return completionSource.Task;
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

	public async Task<bool> Interstitial()
	{
		if (CheckUnavailable())
			return true;
		
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Interstitial())
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

	public async Task<bool> Rewarded()
	{
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Rewarded())
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
