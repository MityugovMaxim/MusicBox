using System;
using System.Threading.Tasks;
using AudioBox.Logging;
using MAXHelper;
using UnityEngine;

[RequireComponent(typeof(AppLovinComp))]
public class MediationManager : MonoBehaviour
{
	const int TIMEOUT = 15000;

	bool InterstitialLoaded => m_Mediation.IsReady(false);
	bool RewardedLoaded     => m_Mediation.IsReady(true);

	public static MediationManager Instance { get; private set; }

	[SerializeField] MAXCustomSettings m_Settings;

	AppLovinComp m_Mediation;

	Action<AdsState> m_AdsFinished;
	Action           m_InterstitialCanceled;
	Action           m_RewardedCanceled;

	void Awake()
	{
		Instance = this;
		
		m_Mediation = GetComponent<AppLovinComp>();
		
		GameObject.DontDestroyOnLoad(gameObject);
	}

	void OnDestroy()
	{
		if (m_Mediation == null)
			return;
		
		m_Mediation.onAdLoadedEvent          -= AdsLoaded;
		m_Mediation.onFinishAdsEvent         -= InvokeAdsFinished;
		m_Mediation.onErrorEvent             -= InvokeAdsFailed;
		m_Mediation.onInterDismissedEvent    -= InvokeInterstitialCanceled;
		m_Mediation.onRewardedDismissedEvent -= InvokeRewardedCanceled;
	}

	public void Initialize()
	{
		m_Mediation.onAdLoadedEvent          += AdsLoaded;
		m_Mediation.onFinishAdsEvent         += InvokeAdsFinished;
		m_Mediation.onErrorEvent             += InvokeAdsFailed;
		m_Mediation.onInterDismissedEvent    += InvokeInterstitialCanceled;
		m_Mediation.onRewardedDismissedEvent += InvokeRewardedCanceled;
		m_Mediation.Init(m_Settings);
	}

	public async Task<bool> WaitInterstitial(int _Attempts = 1)
	{
		int attempt = 0;
		while (attempt < _Attempts)
		{
			if (InterstitialLoaded)
				return true;
			
			await Task.WhenAny(
				UnityTask.Check(() => InterstitialLoaded, 0.1f),
				Task.Delay(TIMEOUT)
			);
			
			attempt++;
		}
		
		return InterstitialLoaded;
	}

	public async Task<bool> WaitRewarded(int _Attempts = 1)
	{
		int attempt = 0;
		while (attempt < _Attempts)
		{
			if (RewardedLoaded)
				return true;
			
			await Task.WhenAny(
				UnityTask.Check(() => RewardedLoaded, 0.1f),
				Task.Delay(TIMEOUT)
			);
			
			attempt++;
		}
		
		return RewardedLoaded;
	}

	public async Task<AdsState> ShowInterstitialAsync()
	{
		await Task.WhenAny(
			UnityTask.Until(() => InterstitialLoaded),
			Task.Delay(TIMEOUT)
		);
		
		if (!InterstitialLoaded)
			return AdsState.Unavailable;
		
		AdsState state = await ShowInterstitial();
		
		if (state == AdsState.Failed && m_Mediation.IsReady(false))
			state = await ShowInterstitial();
		
		return state;
	}

	public async Task<AdsState> ShowRewardedAsync()
	{
		await Task.WhenAny(
			UnityTask.Until(() => RewardedLoaded),
			Task.Delay(TIMEOUT)
		);
		
		AdsState state;
		if (RewardedLoaded)
			state = await ShowRewarded();
		else
			return AdsState.Timeout;
		
		if (state == AdsState.Failed && RewardedLoaded)
			state = await ShowInterstitial();
		
		return state;
	}

	Task<AdsState> ShowInterstitial()
	{
		InvokeAdsFinished(false);
		
		if (!m_Mediation.IsReady(false))
			return Task.FromResult(AdsState.Unavailable);
		
		TaskCompletionSource<AdsState> completionSource = new TaskCompletionSource<AdsState>();
		
		m_AdsFinished          = _State => completionSource.TrySetResult(_State);
		m_InterstitialCanceled = () => completionSource.TrySetResult(AdsState.Canceled);
		
		m_Mediation.ShowInterstitial();
		
		return completionSource.Task;
	}

	Task<AdsState> ShowRewarded()
	{
		InvokeAdsFinished(false);
		
		if (!m_Mediation.IsReady(true))
			return Task.FromResult(AdsState.Unavailable);
		
		TaskCompletionSource<AdsState> completionSource = new TaskCompletionSource<AdsState>();
		
		m_AdsFinished      = _State => completionSource.TrySetResult(_State);
		m_RewardedCanceled = () => completionSource.TrySetResult(AdsState.Canceled);
		
		m_Mediation.ShowRewarded();
		
		return completionSource.Task;
	}

	void InvokeAdsFinished(bool _Success)
	{
		Action<AdsState> action = m_AdsFinished;
		RestoreActions();
		action?.Invoke(_Success ? AdsState.Completed : AdsState.Canceled);
	}

	void InvokeAdsFailed(MaxSdkBase.AdInfo _AdInfo, MaxSdkBase.ErrorInfo _ErrorInfo, bool _Rewarded)
	{
		Action<AdsState> action = m_AdsFinished;
		RestoreActions();
		action?.Invoke(AdsState.Failed);
	}

	void InvokeInterstitialCanceled()
	{
		Action action = m_InterstitialCanceled;
		RestoreActions();
		action?.Invoke();
	}

	void InvokeRewardedCanceled()
	{
		Action action = m_RewardedCanceled;
		RestoreActions();
		action?.Invoke();
	}

	void AdsLoaded(bool _Rewarded)
	{
		if (_Rewarded)
			Log.Info(this, "Rewarded load complete.");
		else
			Log.Info(this, "Interstitial load complete.");
	}

	void RestoreActions()
	{
		m_AdsFinished          = null;
		m_InterstitialCanceled = null;
		m_RewardedCanceled     = null;
	}
}
