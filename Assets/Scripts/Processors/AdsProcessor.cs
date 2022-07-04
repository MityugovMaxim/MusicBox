using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
using Firebase.Database;
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

	string m_InterstitialID;
	string m_RewardedID;

	public Task<bool> Initialize(string _InterstitialID, string _RewardedID)
	{
		AdsManager.Instance.InitApplovin();
		
		m_InterstitialID = _InterstitialID;
		m_RewardedID     = _RewardedID;
		
		return Task.FromResult(true);
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
public class AdsProviderSnapshot
{
	public string ID             { get; }
	public bool   Active         { get; }
	public string InterstitialID { get; }
	public string RewardedID     { get; }

	public AdsProviderSnapshot(DataSnapshot _Data)
	{
		ID     = _Data.Key;
		Active = _Data.GetBool("active");
		#if UNITY_IOS
		InterstitialID = _Data.GetString("ios_interstitial");
		RewardedID     = _Data.GetString("ios_rewarded");
		#elif UNITY_ANDROID
		InterstitialID = _Data.GetString("android_interstitial");
		RewardedID     = _Data.GetString("android_rewarded");
		#endif
	}
}

[Preserve]
public class AdsProcessor
{
	bool Loaded { get; set; }

	[Inject] IAdsProvider[] m_AdsProviders;
	[Inject] AudioManager   m_AudioManager;

	readonly List<AdsProviderSnapshot> m_Snapshots = new List<AdsProviderSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("ads_providers");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		InitializeAdsProviders();
		
		Loaded = true;
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
		return m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	string GetInterstitialID(string _AdsProviderID)
	{
		AdsProviderSnapshot snapshot = GetSnapshot(_AdsProviderID);
		
		return snapshot?.InterstitialID;
	}

	string GetRewardedID(string _AdsProviderID)
	{
		AdsProviderSnapshot snapshot = GetSnapshot(_AdsProviderID);
		
		return snapshot?.RewardedID;
	}

	IAdsProvider GetAdsProvider(string _AdsProviderID)
	{
		if (string.IsNullOrEmpty(_AdsProviderID))
			return null;
		
		return m_AdsProviders.FirstOrDefault(_AdsProvider => _AdsProvider.ID == _AdsProviderID);
	}

	public async Task<bool> Interstitial()
	{
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Interstitial())
				continue;
			
			AudioListener.volume = 1;
			
			m_AudioManager.SetAudioActive(true);
			
			await UnityTask.Yield();
			
			return true;
		}
		
		AudioListener.volume = 1;
		
		m_AudioManager.SetAudioActive(true);
		
		return false;
	}

	public async Task<bool> Interstitial(string _PlacementID)
	{
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Interstitial(_PlacementID))
				continue;
			
			AudioListener.volume = 1;
			
			m_AudioManager.SetAudioActive(true);
			
			await UnityTask.Yield();
			
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
			
			AudioListener.volume = 1;
			
			m_AudioManager.SetAudioActive(true);
			
			await UnityTask.Yield();
			
			return true;
		}
		
		AudioListener.volume = 1;
		
		m_AudioManager.SetAudioActive(true);
		
		return false;
	}

	public async Task<bool> Rewarded(string _PlacementID)
	{
		foreach (IAdsProvider provider in m_AdsProviders)
		{
			AudioListener.volume = 0;
			
			if (!await provider.Rewarded())
				continue;
			
			AudioListener.volume = 1;
			
			m_AudioManager.SetAudioActive(true);
			
			await UnityTask.Yield();
			
			return true;
		}
		
		AudioListener.volume = 1;
		
		m_AudioManager.SetAudioActive(true);
		
		return false;
	}

	void Unload()
	{
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (FirebaseAuth.DefaultInstance.CurrentUser == null)
		{
			Unload();
			return;
		}
		
		Log.Info(this, "Updating ads data...");
		
		await Fetch();
		
		Log.Info(this, "Update ads data complete.");
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch ads data failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new AdsProviderSnapshot(_Data)));
	}

	AdsProviderSnapshot GetSnapshot(string _AdsProviderID)
	{
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _AdsProviderID);
	}
}
