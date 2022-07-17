using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using Unity.Advertisement.IosSupport; // NOTE: Import "com.unity.ads.ios-support" from Package Manager, if it's missing
#endif

namespace MAXHelper {
    public class AppLovinComp : MonoBehaviour {
        #region Fields
        private MaxSdkBase.AdInfo ShowedInfo;
        private MAXCustomSettings Settings;
        private string RewardedID = "empty";
        private string BannerID = "empty";
        private string InterstitialID = "empty";
        private bool bInitialized;
        #endregion


        #region Events Declaration
        public UnityAction<bool> onFinishAdsEvent;
        public UnityAction<MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo, bool> onErrorEvent;
        public UnityAction onRewardedDismissedEvent;
        public UnityAction onInterDismissedEvent;
        public UnityAction OnBannerInitialized;
        public UnityAction<bool> onAdLoadedEvent; // true = rewarded 
        #endregion


        #region Initialization
        public void Init(MAXCustomSettings CustomSettings) {
            Settings = CustomSettings;
            if (string.IsNullOrEmpty(Settings.SDKKey)) {
                Debug.LogError("[MadPixel] Cant init SDK with a null SDK key!");
            }
            else {
                MaxSdkCallbacks.OnSdkInitializedEvent += OnAppLovinInitialized;
                InitSDK();
            }
        }

        private void InitSDK() {
            MaxSdk.SetSdkKey(Settings.SDKKey);
            MaxSdk.InitializeSdk();
        }

        private void OnAppLovinInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration) {
            if (Settings.bShowMediationDebugger) {
                MaxSdk.ShowMediationDebugger();
            }

            switch (sdkConfiguration.ConsentDialogState) {
                case MaxSdkBase.ConsentDialogState.Applies:
                    ShowConsentDialog();
                    break;
                case MaxSdkBase.ConsentDialogState.DoesNotApply:
                    break;
                case MaxSdkBase.ConsentDialogState.Unknown:
                    break;
            }

            MaxSdk.SetHasUserConsent(true);

            if (Settings.bUseBanners) {
                InitializeBannerAds();
            }

            if (Settings.bUseRewardeds) {
                InitializeRewardedAds();
            }

            if (Settings.bUseInters) {
                InitializeInterstitialAds();
            }

            Debug.Log("[MadPixel] AppLovin is initialized");
            bInitialized = true;
        }

        private void ShowConsentDialog() {
#if UNITY_ANDROID
            //MaxSdk.UserService.ShowConsentDialog();
#elif UNITY_IOS
            // NOTE: Handled by https://dash.applovin.com/documentation/mediation/unity/getting-started/consent-flow
            //Debug.Log("Unity iOS Support: Requesting iOS App Tracking Transparency native dialog.");

            //ATTrackingStatusBinding.RequestAuthorizationTracking();
#endif
        }

        #endregion

        #region Banners
        public void InitializeBannerAds() {
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments
#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(Settings.BannerID)) {
                BannerID = Settings.BannerID;
            } else {
                Debug.LogError("[MadPixel] Banner ID in Settings is Empty!");
            }
#else
            if (!string.IsNullOrEmpty(Settings.BannerID_IOS)) {
                BannerID = Settings.BannerID_IOS;
            } else {
                Debug.LogError("Banner ID in Settings is Empty!");
            }
#endif
            MaxSdk.CreateBanner(BannerID, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(BannerID, Settings.BannerBackground);

            OnBannerInitialized?.Invoke();
        }
        #endregion

        #region Interstitials
        public void InitializeInterstitialAds() {
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToDisplayEvent;

            // Load the first interstitial
            LoadInterstitial();
        }

        private void LoadInterstitial() {
#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(Settings.InterstitialID)) {
                InterstitialID = Settings.InterstitialID;
            } else {
                Debug.LogError("[MadPixel] Interstitial ID in Settings is Empty!");
            }
#else
            if (!string.IsNullOrEmpty(Settings.InterstitialID_IOS)) {
                InterstitialID = Settings.InterstitialID_IOS;
            } else {
                Debug.LogError("Interstitial ID in Settings is Empty!");
            }
#endif
            MaxSdk.LoadInterstitial(InterstitialID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            onAdLoadedEvent?.Invoke(false);
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Interstitial ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadInterstitial", 3);
            Debug.LogWarning("OnInterstitialFailedEvent");
        }

        private void OnInterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            // Interstitial ad failed to display. We recommend loading the next ad
            LoadInterstitial();

            onErrorEvent(adInfo, errorInfo, false);
            onInterDismissedEvent?.Invoke();

            Debug.LogWarning("InterstitialFailedToDisplayEvent");
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            // Interstitial ad is hidden. Pre-load the next ad
            LoadInterstitial();

            onInterDismissedEvent?.Invoke();
        }
        #endregion

        public void InitializeInterstitial(string _PlacementID)
        {
            Settings.bUseInters         = true;
            Settings.InterstitialID     = _PlacementID;
            Settings.InterstitialID_IOS = _PlacementID;
            
            InitializeInterstitialAds();
        }

        public void InitializeRewarded(string _PlacementID)
        {
            Settings.bUseRewardeds  = true;
            Settings.RewardedID     = _PlacementID;
            Settings.RewardedID_IOS = _PlacementID;
            
            InitializeRewardedAds();
        }

        #region Rewarded
        public void InitializeRewardedAds() {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first RewardedAd
            LoadRewardedAd();
        }

        public void CancelRewardedAd() {
            onFinishAdsEvent?.Invoke(false);
            onRewardedDismissedEvent?.Invoke();
            ShowedInfo = null;
        }

        private void LoadRewardedAd() {
#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(Settings.RewardedID)) {
                RewardedID = Settings.RewardedID;
            } else {
                Debug.LogError("[MadPixel] Rewarded ID in Settings is Empty!");
            }
#else
            if (!string.IsNullOrEmpty(Settings.RewardedID_IOS)) {
                RewardedID = Settings.RewardedID_IOS;
            } else {
                Debug.LogError("Rewarded ID in Settings is Empty!");
            }
#endif
            MaxSdk.LoadRewardedAd(RewardedID);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            ShowedInfo = adInfo;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            onAdLoadedEvent?.Invoke(true);
            ShowedInfo = adInfo;
            //Debug.LogWarning("OnRewardedAd LoadedEvent");
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadRewardedAd", 3);
            //Debug.LogWarning("OnRewardedAdFailedEvent");
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            // Rewarded ad failed to display. We recommend loading the next ad

            //Debug.LogWarning("RewardedAd Failed To Display! Error " + errorInfo.ToString());

            OnError(adInfo, errorInfo);
            LoadRewardedAd();
        }

        private void OnError(MaxSdkBase.AdInfo adInfo, MaxSdkBase.ErrorInfo EInfo) {
            onErrorEvent?.Invoke(adInfo, EInfo, true);
            ShowedInfo = null;
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            //Debug.LogWarning("REWARDED DISMISSED");
            if (ShowedInfo != null) {
                onFinishAdsEvent?.Invoke(false);
            }

            onRewardedDismissedEvent?.Invoke();
            ShowedInfo = null;
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo) {
            //Debug.LogWarning($"REWARDED get your reward with result! {ShowedInfo != null}");
            onFinishAdsEvent?.Invoke(ShowedInfo != null);
            ShowedInfo = null;
        }

        #endregion

        #region Show Ads

        public bool ShowInterstitial() {
            if (bInitialized && MaxSdk.IsInterstitialReady(InterstitialID)) {
                MaxSdk.ShowInterstitial(InterstitialID);
                return true;
            }

            return false;
        }

        public void ShowRewarded() {
            if (bInitialized && MaxSdk.IsRewardedAdReady(RewardedID)) {
                MaxSdk.ShowRewardedAd(RewardedID);
            }
        }

        public bool IsReady(bool bIsRewarded) {
            if (bInitialized) {
                if (bIsRewarded) {
                    return MaxSdk.IsRewardedAdReady(RewardedID);
                }
                else {
                    return MaxSdk.IsInterstitialReady(InterstitialID);
                }
            }

            return false;
        }
        #endregion

        #region Banners
        public void ShowBanner(bool bShow) {
            if (bInitialized) {
                if (bShow) {
                    MaxSdk.ShowBanner(BannerID);
                }
                else {
                    MaxSdk.HideBanner(BannerID);
                }
            }
        }
        #endregion

        #region Unsubscribers

        void OnDestroy() {
            UnsubscribeAll();
        }
        public void UnsubscribeAll() {
            if (bInitialized) {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialDismissedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialFailedToDisplayEvent;

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdDismissedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
            }
        }

        #endregion

        #region If Unity Editor
        public void ForceFinishAds(bool Success) {
            onFinishAdsEvent?.Invoke(Success);
        }

        public void ForceFinishInters() {
            onInterDismissedEvent?.Invoke();
        }

        public void ForceError() {
            OnError(ShowedInfo, null);
        }
        public IEnumerator ImitateNewAdLoad() {
            yield return new WaitForSeconds(2f);
            onAdLoadedEvent?.Invoke(true);
        }
        #endregion

    }
}