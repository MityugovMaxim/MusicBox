using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Advertisements;

public abstract class AdsProcessor : IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
	static bool TestMode
	{
		#if DEVELOPMENT_BUILD
		get => true;
		#else
		get => false;
		#endif
	}

	protected abstract string GameID         { get; }
	protected abstract string InterstitialID { get; }
	protected abstract string RewardedID     { get; }

	readonly ProfileProcessor m_ProfileProcessor;

	readonly Dictionary<string, Action<bool>> m_LoadFinished = new Dictionary<string, Action<bool>>();
	readonly Dictionary<string, Action<bool>> m_ShowFinished  = new Dictionary<string, Action<bool>>();

	Action<bool> m_LoadAdsFinished;
	Action<bool> m_InterstitialFinished;
	Action<bool> m_RewardedFinished;

	protected AdsProcessor(ProfileProcessor _ProfileProcessor)
	{
		m_ProfileProcessor = _ProfileProcessor;
	}

	public Task<bool> LoadAds()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_LoadAdsFinished = _Success => completionSource.TrySetResult(_Success);
		
		if (Advertisement.isInitialized)
		{
			InvokeLoadAdsFinished(true);
			return completionSource.Task;
		}
		
		if (!Advertisement.isSupported)
		{
			Debug.LogError("[AdsProcessor] Load ads failed. Ads not supported.");
			InvokeLoadAdsFinished(false);
			return completionSource.Task;
		}
		
		Advertisement.Initialize(GameID, TestMode, this);
		
		return completionSource.Task;
	}

	public async Task<bool> Interstitial(bool _Force = false)
	{
		if (!_Force && m_ProfileProcessor.HasNoAds())
			return true;
		
		return await Show(InterstitialID);
	}

	public async Task<bool> Rewarded(bool _Force = false)
	{
		if (!_Force && m_ProfileProcessor.HasNoAds())
			return true;
		
		return await Show(RewardedID);
	}

	Task<bool> Load(string _PlacementID)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_LoadFinished[_PlacementID] = _Success => completionSource.TrySetResult(_Success);
		
		Advertisement.Load(_PlacementID, this);
		
		return completionSource.Task;
	}

	async Task<bool> Show(string _PlacementID)
	{
		if (!Advertisement.isInitialized)
			await LoadAds();
		
		bool loaded = await Load(_PlacementID);
		
		if (!loaded)
		{
			Debug.LogFormat("[AdsProcessor] Second attempt to load placement. Placement: {0}.", _PlacementID);
			
			await Task.Delay(250);
			
			loaded = await Load(_PlacementID);
		}
		
		if (!loaded)
			return false;
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_ShowFinished[_PlacementID] = _Success => completionSource.TrySetResult(_Success);
		
		Advertisement.Show(_PlacementID, this);
		
		await completionSource.Task;
		
		await Task.Delay(250);
		
		return completionSource.Task.Result;
	}

	void IUnityAdsInitializationListener.OnInitializationComplete()
	{
		Debug.Log("[AdsProcessor] Load ads success.");
		
		InvokeLoadAdsFinished(true);
	}

	void IUnityAdsInitializationListener.OnInitializationFailed(UnityAdsInitializationError _Error, string _Message)
	{
		Debug.LogErrorFormat("[AdsProcessor] Load ads failed. Error: {0} Message: {1}.", _Error, _Message);
		
		InvokeLoadAdsFinished(false);
	}

	void IUnityAdsLoadListener.OnUnityAdsAdLoaded(string _PlacementID)
	{
		Debug.LogFormat("[AdsProcessor] Load placement success. Placement: {0}.", _PlacementID);
		
		InvokeLoadFinished(_PlacementID, true);
	}

	void IUnityAdsLoadListener.OnUnityAdsFailedToLoad(string _PlacementID, UnityAdsLoadError _Error, string _Message)
	{
		Debug.LogErrorFormat("[AdsProcessor] Load placement failed. Placement: {0} Error: {1} Message: {2}.", _PlacementID, _Error, _Message);
		
		InvokeLoadFinished(_PlacementID, false);
	}

	void IUnityAdsShowListener.OnUnityAdsShowStart(string _PlacementID)
	{
		Debug.LogFormat("[AdsProcessor] Show placement. Placement: {0}.", _PlacementID);
	}

	void IUnityAdsShowListener.OnUnityAdsShowClick(string _PlacementID)
	{
		Debug.LogFormat("[AdsProcessor] Click placement. Placement: {0}.", _PlacementID);
	}

	void IUnityAdsShowListener.OnUnityAdsShowComplete(string _PlacementID, UnityAdsShowCompletionState _State)
	{
		Debug.LogFormat("[AdsProcessor] Show placement success. Placement: {0} State: {1}.", _PlacementID, _State);
		
		InvokeShowFinished(_PlacementID, _State == UnityAdsShowCompletionState.COMPLETED);
	}

	void IUnityAdsShowListener.OnUnityAdsShowFailure(string _PlacementID, UnityAdsShowError _Error, string _Message)
	{
		Debug.LogErrorFormat("[AdsProcessor] Show placement failed. Placement: {0} Error: {1} Message: {2}.", _PlacementID, _Error, _Message);
		
		InvokeShowFinished(_PlacementID, false);
	}

	void InvokeLoadAdsFinished(bool _Success)
	{
		Action<bool> action = m_LoadAdsFinished;
		m_LoadAdsFinished = null;
		action?.Invoke(_Success);
	}

	void InvokeLoadFinished(string _PlacementID, bool _Success)
	{
		if (!m_LoadFinished.TryGetValue(_PlacementID, out Action<bool> action))
			return;
		
		m_LoadFinished.Remove(_PlacementID);
		
		action?.Invoke(_Success);
	}

	void InvokeShowFinished(string _PlacementID, bool _Success)
	{
		if (!m_ShowFinished.TryGetValue(_PlacementID, out Action<bool> action))
			return;
		
		m_ShowFinished.Remove(_PlacementID);
		
		action?.Invoke(_Success);
	}
}
