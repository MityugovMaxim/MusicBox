using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
using Firebase.Database;
using MAXHelper;
using UnityEngine;
//using UnityEngine.Advertisements;
using UnityEngine.Scripting;
using Zenject;

public interface IAdsProvider
{
	string ID { get; }
	Task<bool> Initialize(string _InterstitialID, string _RewardedID);
	Task<bool> Interstitial();
	Task<bool> Rewarded();
}

[Preserve]
public class AdsProviderMadPixel : IAdsProvider
{
	public string ID => "mad_pixel";

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
		if (string.IsNullOrEmpty(m_InterstitialID))
			return Task.FromResult(false);
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		AdsManager.Instance.ShowAd(
			_Success => completionSource.TrySetResult(_Success),
			() => completionSource.TrySetResult(false),
			m_InterstitialID,
			false
		);
		
		return completionSource.Task;
	}

	public Task<bool> Rewarded()
	{
		if (string.IsNullOrEmpty(m_RewardedID))
			return Task.FromResult(false);
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		AdsManager.Instance.ShowAd(
			_Success => completionSource.TrySetResult(_Success),
			() => completionSource.TrySetResult(false),
			m_RewardedID,
			true
		);
		
		return completionSource.Task;
	}
}

[Preserve]
public class AdsProviderAdMob : IAdsProvider
{
	string IAdsProvider.ID => "ad_mob";

	public Task<bool> Initialize(string _InterstitialID, string _RewardedID)
	{
		return Task.FromResult(false);
	}

	Task<bool> IAdsProvider.Interstitial()
	{
		return Task.FromResult(false);
	}

	Task<bool> IAdsProvider.Rewarded()
	{
		return Task.FromResult(false);
	}
}

// [Preserve]
// public class AdsProviderUnity : IAdsProvider, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
// {
// 	#if UNITY_IOS
// 	static string GameID         => "4234912";
// 	static string InterstitialID => "Interstitial_iOS";
// 	static string RewardedID     => "Rewarded_iOS";
// 	#elif UNITY_ANDROID
// 	static string GameID         => "4234913";
// 	static string InterstitialID => "Interstitial_Android";
// 	static string RewardedID     => "Rewarded_Android";
// 	#endif
//
// 	string IAdsProvider.ID => "unity_ads";
//
// 	readonly Dictionary<string, Action<bool>> m_LoadFinished = new Dictionary<string, Action<bool>>();
// 	readonly Dictionary<string, Action<bool>> m_ShowFinished = new Dictionary<string, Action<bool>>();
//
// 	Action<bool> m_LoadAdsFinished;
// 	Action<bool> m_InterstitialFinished;
// 	Action<bool> m_RewardedFinished;
//
// 	public Task<bool> Initialize()
// 	{
// 		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
// 		
// 		m_LoadAdsFinished = _Success => completionSource.TrySetResult(_Success);
// 		
// 		if (Advertisement.isInitialized)
// 		{
// 			InvokeLoadAdsFinished(true);
// 			return completionSource.Task;
// 		}
// 		
// 		if (!Advertisement.isSupported)
// 		{
// 			Debug.LogError("[AdsUnity] Initialize failed. Ads not supported.");
// 			InvokeLoadAdsFinished(false);
// 			return completionSource.Task;
// 		}
// 		
// 		#if UNITY_EDITOR || DEVELOPMENT_BUILD
// 		Advertisement.Initialize(GameID, true, this);
// 		#else
// 		Advertisement.Initialize(GameID, false, this);
// 		#endif
// 		
// 		return completionSource.Task;
// 	}
//
// 	Task<bool> IAdsProvider.Interstitial()
// 	{
// 		return Show(InterstitialID);
// 	}
//
// 	Task<bool> IAdsProvider.Rewarded()
// 	{
// 		return Show(RewardedID);
// 	}
//
// 	async Task<bool> Show(string _PlacementID)
// 	{
// 		if (!Advertisement.isInitialized)
// 			await Initialize();
// 		
// 		bool loaded = await Load(_PlacementID);
// 		
// 		if (!loaded)
// 		{
// 			Debug.LogFormat("[AdsProcessor] Second attempt to load placement. Placement: {0}.", _PlacementID);
// 			
// 			await Task.Delay(250);
// 			
// 			loaded = await Load(_PlacementID);
// 		}
// 		
// 		if (!loaded)
// 			return false;
// 		
// 		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
// 		
// 		m_ShowFinished[_PlacementID] = _Success => completionSource.TrySetResult(_Success);
// 		
// 		Advertisement.Show(_PlacementID, this);
// 		
// 		await completionSource.Task;
// 		
// 		await Task.Delay(250);
// 		
// 		return completionSource.Task.Result;
// 	}
//
// 	Task<bool> Load(string _PlacementID)
// 	{
// 		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
// 		
// 		m_LoadFinished[_PlacementID] = _Success => completionSource.TrySetResult(_Success);
// 		
// 		Advertisement.Load(_PlacementID, this);
// 		
// 		return completionSource.Task;
// 	}
//
// 	void InvokeLoadAdsFinished(bool _Success)
// 	{
// 		Action<bool> action = m_LoadAdsFinished;
// 		m_LoadAdsFinished = null;
// 		action?.Invoke(_Success);
// 	}
//
// 	void InvokeLoadFinished(string _PlacementID, bool _Success)
// 	{
// 		if (!m_LoadFinished.TryGetValue(_PlacementID, out Action<bool> action))
// 			return;
// 		
// 		m_LoadFinished.Remove(_PlacementID);
// 		
// 		action?.Invoke(_Success);
// 	}
//
// 	void InvokeShowFinished(string _PlacementID, bool _Success)
// 	{
// 		if (!m_ShowFinished.TryGetValue(_PlacementID, out Action<bool> action))
// 			return;
// 		
// 		m_ShowFinished.Remove(_PlacementID);
// 		
// 		action?.Invoke(_Success);
// 	}
//
// 	#region Unity Ads Implementation
//
// 	void IUnityAdsInitializationListener.OnInitializationComplete()
// 	{
// 		Debug.Log("[AdsProcessor] Load ads success.");
// 		
// 		InvokeLoadAdsFinished(true);
// 	}
//
// 	void IUnityAdsInitializationListener.OnInitializationFailed(UnityAdsInitializationError _Error, string _Message)
// 	{
// 		Debug.LogErrorFormat("[AdsProcessor] Load ads failed. Error: {0} Message: {1}.", _Error, _Message);
// 		
// 		InvokeLoadAdsFinished(false);
// 	}
//
// 	void IUnityAdsLoadListener.OnUnityAdsAdLoaded(string _PlacementID)
// 	{
// 		Debug.LogFormat("[AdsProcessor] Load placement success. Placement: {0}.", _PlacementID);
// 		
// 		InvokeLoadFinished(_PlacementID, true);
// 	}
//
// 	void IUnityAdsLoadListener.OnUnityAdsFailedToLoad(string _PlacementID, UnityAdsLoadError _Error, string _Message)
// 	{
// 		Debug.LogErrorFormat("[AdsProcessor] Load placement failed. Placement: {0} Error: {1} Message: {2}.", _PlacementID, _Error, _Message);
// 		
// 		InvokeLoadFinished(_PlacementID, false);
// 	}
//
// 	void IUnityAdsShowListener.OnUnityAdsShowStart(string _PlacementID)
// 	{
// 		Debug.LogFormat("[AdsProcessor] Show placement. Placement: {0}.", _PlacementID);
// 	}
//
// 	void IUnityAdsShowListener.OnUnityAdsShowClick(string _PlacementID)
// 	{
// 		Debug.LogFormat("[AdsProcessor] Click placement. Placement: {0}.", _PlacementID);
// 	}
//
// 	void IUnityAdsShowListener.OnUnityAdsShowComplete(string _PlacementID, UnityAdsShowCompletionState _State)
// 	{
// 		Debug.LogFormat("[AdsProcessor] Show placement success. Placement: {0} State: {1}.", _PlacementID, _State);
// 		
// 		InvokeShowFinished(_PlacementID, _State == UnityAdsShowCompletionState.COMPLETED);
// 	}
//
// 	void IUnityAdsShowListener.OnUnityAdsShowFailure(string _PlacementID, UnityAdsShowError _Error, string _Message)
// 	{
// 		Debug.LogErrorFormat("[AdsProcessor] Show placement failed. Placement: {0} Error: {1} Message: {2}.", _PlacementID, _Error, _Message);
// 		
// 		InvokeShowFinished(_PlacementID, false);
// 	}
//
// 	#endregion
// }

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
