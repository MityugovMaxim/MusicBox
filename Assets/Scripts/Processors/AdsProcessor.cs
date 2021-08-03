using System;
using UnityEngine;
using UnityEngine.Advertisements;
using Zenject;

public abstract class AdsProcessor : IInitializable, IUnityAdsListener
{
	static bool TestMode
	{
		get
		{
			#if UNITY_EDITOR || DEVELOPMENT_BUILD
			return true;
			#else
			return false;
			#endif
		}
	}

	protected abstract string GameID         { get; }
	protected abstract string InterstitialID { get; }
	protected abstract string RewardedID     { get; }

	bool m_InterstitialLoaded;
	bool m_RewardedLoaded;

	Action m_InterstitialFinished;
	Action m_RewardedSuccess;
	Action m_RewardedFailed;

	void IInitializable.Initialize()
	{
		if (!Advertisement.isSupported)
		{
			Debug.LogError("[AdsProcessor] Ads initialization failed. Ads not supported.");
			return;
		}
		
		Advertisement.Initialize(GameID, TestMode, false);
		Advertisement.AddListener(this);
		Advertisement.Load(InterstitialID);
		Advertisement.Load(RewardedID);
	}

	public void ShowInterstitial(Action _Finished = null)
	{
		if (!Advertisement.isInitialized)
		{
			Debug.LogError("[AdsProcessor] Show interstitial failed. Ads not initialized.");
			_Finished?.Invoke();
			return;
		}
		
		if (!m_InterstitialLoaded)
		{
			Debug.LogError("[AdsProcessor] Show interstitial failed. Interstitial not loaded.");
			_Finished?.Invoke();
			return;
		}
		
		if (Advertisement.isShowing)
		{
			Debug.LogError("[AdsProcessor] Show interstitial failed. Ads already showing.");
			_Finished?.Invoke();
			return;
		}
		
		InvokeInterstitialFinished();
		
		m_InterstitialFinished = _Finished;
		
		Advertisement.Show(InterstitialID);
	}

	public void ShowRewarded(Action _Success = null, Action _Failed = null)
	{
		if (!Advertisement.isInitialized)
		{
			Debug.LogError("[AdsProcessor] Show rewarded failed. Ads not initialized.");
			_Failed?.Invoke();
			return;
		}
		
		if (!m_RewardedLoaded)
		{
			Debug.LogError("[AdsProcessor] Show rewarded failed. Rewarded not loaded.");
			return;
		}
		
		if (Advertisement.isShowing)
		{
			Debug.LogError("[AdsProcessor] Show rewarded failed. Ads already showing.");
			return;
		}
		
		InvokeRewardedFailed();
		
		m_RewardedSuccess = _Success;
		m_RewardedFailed  = _Failed;
		
		Advertisement.Show(RewardedID);
	}

	void IUnityAdsListener.OnUnityAdsReady(string _PlacementID)
	{
		Debug.LogFormat("[AdsProcessor] Ads ready. Placement: {0}.", _PlacementID);
		
		if (_PlacementID == InterstitialID)
			m_InterstitialLoaded = true;
		else if (_PlacementID == RewardedID)
			m_RewardedLoaded = true;
	}

	void IUnityAdsListener.OnUnityAdsDidError(string _Message)
	{
		Debug.LogErrorFormat("[AdsProcessor] Ads initialization failed. {0}.", _Message);
	}

	void IUnityAdsListener.OnUnityAdsDidStart(string _PlacementID)
	{
		Debug.LogFormat("[AdsProcessor] Ads started. Placement: {0}.", _PlacementID);
	}

	void IUnityAdsListener.OnUnityAdsDidFinish(string _PlacementID, ShowResult _Result)
	{
		Debug.LogFormat("[AdsProcessor] Ads finished. Placement: {0}.", _PlacementID);
		
		if (_PlacementID == RewardedID)
		{
			switch (_Result)
			{
				case ShowResult.Finished:
					InvokeRewardedSuccess();
					break;
				case ShowResult.Failed:
				case ShowResult.Skipped:
					InvokeRewardedFailed();
					break;
			}
		}
		else if (_PlacementID == InterstitialID)
		{
			InvokeInterstitialFinished();
		}
	}

	void InvokeRewardedSuccess()
	{
		Action action = m_RewardedSuccess;
		m_RewardedSuccess = null;
		action?.Invoke();
	}

	void InvokeRewardedFailed()
	{
		Action action = m_RewardedFailed;
		m_RewardedFailed = null;
		action?.Invoke();
	}

	void InvokeInterstitialFinished()
	{
		Action action = m_InterstitialFinished;
		m_InterstitialFinished = null;
		action?.Invoke();
	}
}