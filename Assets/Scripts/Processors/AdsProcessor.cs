using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using Zenject;

public abstract class AdsProcessor : IInitializable, IUnityAdsInitializationListener, IUnityAdsListener
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

	PurchaseProcessor m_PurchaseProcessor;
	ProductInfo       m_NoAdsProduct;

	bool m_InterstitialLoaded;
	bool m_RewardedLoaded;

	Action m_OnInterstitialLoaded;
	Action m_OnRewardedLoaded;
	Action m_InterstitialFinished;
	Action m_RewardedSuccess;
	Action m_RewardedFailed;

	[Inject]
	public void Construct(PurchaseProcessor _PurchaseProcessor, ProductInfo _NoAdsProduct)
	{
		m_PurchaseProcessor = _PurchaseProcessor;
		m_NoAdsProduct      = _NoAdsProduct;
	}

	void IInitializable.Initialize()
	{
		Reload();
	}

	public void Reload()
	{
		if (m_PurchaseProcessor.IsProductPurchased(m_NoAdsProduct.ID))
			return;
		
		if (!Advertisement.isSupported)
		{
			Debug.LogError("[AdsProcessor] Ads initialization failed. Ads not supported.");
			return;
		}
		
		Advertisement.Initialize(GameID, TestMode, true, this);
		Advertisement.AddListener(this);
	}

	public void ShowInterstitial(Action _Finished = null)
	{
		if (m_PurchaseProcessor.IsProductPurchased(m_NoAdsProduct.ID))
		{
			_Finished?.Invoke();
			return;
		}
		
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
		if (m_PurchaseProcessor.IsProductPurchased(m_NoAdsProduct.ID))
		{
			_Success?.Invoke();
			return;
		}
		
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

	public void ShowInterstitialAsync(
		MonoBehaviour _Context,
		Action        _Finished = null
	)
	{
		if (m_PurchaseProcessor.IsProductPurchased(m_NoAdsProduct.ID))
		{
			_Finished?.Invoke();
			return;
		}
		
		_Context.StartCoroutine(ShowInterstitialRoutine(_Finished));
	}

	public void ShowRewardedAsync(
		MonoBehaviour _Context,
		Action        _Success = null,
		Action        _Failed  = null,
		Action        _Cancel  = null
	)
	{
		if (m_PurchaseProcessor.IsProductPurchased(m_NoAdsProduct.ID))
		{
			_Success?.Invoke();
			return;
		}
		
		_Context.StartCoroutine(ShowRewardedRoutine(_Success, _Failed, _Cancel));
	}

	void IUnityAdsInitializationListener.OnInitializationComplete()
	{
		Debug.Log("[AdsProcessor] Ads initialized.");
	}

	void IUnityAdsInitializationListener.OnInitializationFailed(UnityAdsInitializationError _Error, string _Message)
	{
		Debug.LogErrorFormat("[AdsProcessor] Ads initialization failed. Error: {0} Message: {1}.", _Error, _Message);
	}

	void IUnityAdsListener.OnUnityAdsReady(string _PlacementID)
	{
		Debug.LogFormat("[AdsProcessor] Ads placement ready. Placement: {0}.", _PlacementID);
		
		if (_PlacementID == InterstitialID)
			m_InterstitialLoaded = true;
		else if (_PlacementID == RewardedID)
			m_RewardedLoaded = true;
	}

	void IUnityAdsListener.OnUnityAdsDidError(string _Message)
	{
		Debug.LogErrorFormat("[AdsProcessor] Ads placement error. {0}.", _Message);
		
		Reload();
	}

	void IUnityAdsListener.OnUnityAdsDidStart(string _PlacementID)
	{
		Debug.LogFormat("[AdsProcessor] Ads started. Placement: {0}.", _PlacementID);
	}

	void IUnityAdsListener.OnUnityAdsDidFinish(string _PlacementID, ShowResult _Result)
	{
		Debug.LogFormat("[AdsProcessor] Ads finished. Placement: {0} Result: {1}.", _PlacementID, _Result);
		
		if (_PlacementID == RewardedID)
		{
			switch (_Result)
			{
				case ShowResult.Finished:
					InvokeRewardedSuccess();
					break;
				default:
					Debug.LogError("[AdsProcessor] Placement state: " + Advertisement.GetPlacementState(_PlacementID));
					InvokeRewardedFailed();
					break;
			}
		}
		else if (_PlacementID == InterstitialID)
		{
			if (_Result != ShowResult.Finished)
				Debug.LogError("[AdsProcessor] Placement state: " + Advertisement.GetPlacementState(_PlacementID));
			InvokeInterstitialFinished();
		}
	}

	IEnumerator ShowInterstitialRoutine(Action _Finished)
	{
		const float timeout = 30;
		
		float time = 0;
		
		m_InterstitialLoaded = false;
		
		Advertisement.Load(InterstitialID);
		
		yield return new WaitForSeconds(0.5f);
		
		while (time < timeout)
		{
			if (Advertisement.isInitialized && m_InterstitialLoaded)
			{
				yield return WaitInterstitialRoutine(_Finished);
				yield break;
			}
			
			time += Time.deltaTime;
			
			yield return null;
		}
		
		Reload();
		
		_Finished?.Invoke();
	}

	IEnumerator WaitInterstitialRoutine(Action _Finished)
	{
		bool finished = false;
		
		ShowInterstitial(() => finished = true);
		
		yield return new WaitUntil(() => finished);
		
		yield return null;
		
		AudioManager.SetAudioActive(true);
		
		yield return null;
		
		_Finished?.Invoke();
	}

	IEnumerator ShowRewardedRoutine(Action _Success, Action _Failed, Action _Cancel)
	{
		const float timeout = 30;
		
		float time = 0;
		
		m_RewardedLoaded = false;
		
		Advertisement.Load(RewardedID);
		
		yield return new WaitForSeconds(0.5f);
		
		while (time < timeout)
		{
			if (Advertisement.isInitialized && m_RewardedLoaded)
			{
				yield return WaitRewardedRoutine(_Success, _Failed);
				yield break;
			}
			
			time += Time.deltaTime;
			
			yield return null;
		}
		
		Reload();
		
		_Cancel?.Invoke();
	}

	IEnumerator WaitRewardedRoutine(Action _Success, Action _Failed)
	{
		bool success = false;
		bool failed  = false;
		
		ShowRewarded(() => success = true, () => failed = true);
		
		yield return new WaitUntil(() => success || failed);
		
		yield return null;
		
		AudioManager.SetAudioActive(true);
		
		yield return null;
		
		if (success)
			_Success?.Invoke();
		
		if (failed)
			_Failed?.Invoke();
	}

	void InvokeRewardedSuccess()
	{
		Action action = m_RewardedSuccess;
		m_RewardedFailed = null;
		m_RewardedSuccess = null;
		action?.Invoke();
	}

	void InvokeRewardedFailed()
	{
		Action action = m_RewardedFailed;
		m_RewardedSuccess = null;
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
