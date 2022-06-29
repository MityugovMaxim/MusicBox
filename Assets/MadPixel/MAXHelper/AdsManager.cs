using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MAXHelper {
    public class AdsManager : MonoBehaviour {
        private const string version = "1.0.3";
        #region Fields

        [SerializeField] private bool bInitializeOnStart = true;
        [SerializeField] private MAXCustomSettings CustomSettings;
        [SerializeField] private int CooldownBetweenInterstitials = 30;

#if UNITY_EDITOR
        private bool ForceErrorInAdsInEditor = false;
#endif


        private bool bCanShowBanner = true;
        private bool bIntersOn = true;

        private AppLovinComp AppLovin;
        private AdInfo CurrentAdInfo;
        private float LastInterShown;

        #endregion

        #region Events Declaration (Can be used for Analytics)

        public UnityAction OnNewRewardedLoaded;
        public UnityAction<MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo, AdInfo> OnAdDisplayError;
        public UnityAction<AdInfo> OnAdShown;
        public UnityAction<AdInfo> OnAdAvailable;
        public UnityAction<AdInfo> OnAdStarted;
        #endregion

        #region Static
        protected static AdsManager _instance;

        public static bool Exist {
            get { return (_instance != null); }
        }

        public static AdsManager Instance {
            get {
                if (_instance == null) {
                    Debug.LogError("AdsManager wasn't created yet!");

                    GameObject go = new GameObject();
                    go.name = "AdsManager";
                    _instance = go.AddComponent(typeof(AdsManager)) as AdsManager;
                }
                return _instance;
            }
        }

        public static void Destroy(bool immediate = false) {
            if (_instance != null && _instance.gameObject != null) {
                if (immediate) {
                    DestroyImmediate(_instance.gameObject);
                } else {
                    GameObject.Destroy(_instance.gameObject);
                }
            }
            _instance = null;
        }
        #endregion

        #region Init
        public void InitApplovin() {
            LastInterShown = -CooldownBetweenInterstitials;

            AppLovin.Init(CustomSettings);
            AppLovin.onFinishAdsEvent += AppLovin_OnFinishAds;
            AppLovin.onAdLoadedEvent += AppLovin_OnAdLoaded;
            AppLovin.onInterDismissedEvent += AppLovin_OnInterDismissed;
            AppLovin.onErrorEvent += AppLovin_OnError;
        }
        #endregion

        #region Event Catchers
        private void AppLovin_OnAdLoaded(bool IsRewarded) {
            if (IsRewarded) {
                OnNewRewardedLoaded?.Invoke();
            }
        }

        private void AppLovin_OnFinishAds(bool IsFinished) {
            if (CurrentAdInfo == null) return;

            CurrentAdInfo.Availability = IsFinished ? "watched" : "canceled";
            OnAdShown?.Invoke(CurrentAdInfo);

            RestartInterCooldown();

            CurrentAdInfo = null;
            //NOTE: Temporary disable sounds - off
        }

        private void AppLovin_OnInterDismissed() {
            RestartInterCooldown();

            if (CurrentAdInfo != null) {
                OnAdShown?.Invoke(CurrentAdInfo);
            }

            CurrentAdInfo = null;
            //NOTE: Temporary disable sounds - off
        }

        private void AppLovin_OnError(MaxSdkBase.AdInfo adInfo, MaxSdkBase.ErrorInfo EInfo, bool bIsRewarded) {
            if (CurrentAdInfo != null) {
                OnAdDisplayError?.Invoke(adInfo, EInfo, CurrentAdInfo);
            }

            string Placement = CurrentAdInfo == null ? "unknown" : CurrentAdInfo.Placement;


            //#if UNITY_EDITOR
            //            StartCoroutine(AppLovin.ImitateNewAdLoad());
            //#endif
#if UNITY_ANDROID
            if (EInfo.Code == MaxSdkBase.ErrorCode.DontKeepActivitiesEnabled) { // NOTE: User won't see any ads in this session anyway
                return;
            }
#endif

            if (AppLovin.IsReady(true)) {
                ShowAdInner(true, Placement);
            } else {
                //todo add error panel
            }
        }

        private void OnAdErrorPanelDismissed(bool bRetry) {
            if (bRetry && AppLovin.IsReady(true)) {
                CurrentAdInfo.Availability = "waited";
                OnAdAvailable?.Invoke(CurrentAdInfo);
                AppLovin.ShowRewarded();
            } else {
                AppLovin.CancelRewardedAd();
            }
        }
        #endregion

        #region Unity Events

        private void Awake() {
            if (_instance == null) {
                _instance = this;
                GameObject.DontDestroyOnLoad(this.gameObject);

                AppLovin = GetComponent<AppLovinComp>();
                if (AppLovin == null) {
                    AppLovin = gameObject.AddComponent<AppLovinComp>();
                }

                if (bInitializeOnStart) {
                    InitApplovin();
                }
            }
            else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"Two AdsManagers at the same time!");
            }
        }
        private void OnDestroy() {
            if (AppLovin != null) {
                AppLovin.onFinishAdsEvent -= AppLovin_OnFinishAds;
                AppLovin.onInterDismissedEvent -= AppLovin_OnInterDismissed;
                AppLovin.onAdLoadedEvent -= AppLovin_OnAdLoaded;
                AppLovin.onErrorEvent -= AppLovin_OnError;
            }
        }

        #endregion

        #region Public
        public bool ShowAd(UnityAction<bool> OnFinishAds, UnityAction OnAdDismissed, string Placement, bool bRewarded = true) {
            if (HasLoadedAd(bRewarded)) {
                if (bRewarded) {
                    Subscribe(OnFinishAds, OnAdDismissed, true);
                } else {
                    Subscribe(OnAdDismissed);
                }

                ShowAdInner(bRewarded, Placement);
                return true;
            } else {
                StartCoroutine(Ping(OnPingComplete));
                return false;
            }
        }

        public bool HasLoadedAd(bool bIsRewarded = true) {
            return (bIsRewarded || IntersAllowedToShow()) && AppLovin.IsReady(bIsRewarded);
        }

        public void Subscribe(UnityAction<bool> OnFinishAds, UnityAction OnAdCancelled, bool toSubscribe = true) {
            if (toSubscribe) {
                AppLovin.onFinishAdsEvent += OnFinishAds;
                AppLovin.onRewardedDismissedEvent += OnAdCancelled;
            } else {
                AppLovin.onFinishAdsEvent -= OnFinishAds;
                AppLovin.onRewardedDismissedEvent -= OnAdCancelled;
            }
        }

        public void Subscribe(UnityAction OnInterDismissed, bool toSubscribe = true) {
            if (toSubscribe) {
                AppLovin.onInterDismissedEvent += OnInterDismissed;
            } else {
                AppLovin.onInterDismissedEvent -= OnInterDismissed;
            }
        }
        public void CancelAllAds() { // NOTE: On AdsFree bought or On AdsFree checked at game start
            bIntersOn = false;
            bCanShowBanner = false;
            ShowHideBanner(false);
        }

        public void ShowHideBanner(bool bShowHide) {
            if (bShowHide && bCanShowBanner) {
                AppLovin?.ShowBanner(true);
            } else {
                AppLovin?.ShowBanner(false);
            }
        }
        #endregion

        #region Helpers

        private void ShowAdInner(bool bIsRewarded, string Placement) {
#if UNITY_EDITOR
            if (ForceErrorInAdsInEditor && bIsRewarded) {
                AppLovin.ForceError();
                return;
            }
#endif
            if (bIsRewarded || (IntersAllowedToShow())) {
                CurrentAdInfo = new AdInfo(Placement, bIsRewarded);
                OnAdAvailable?.Invoke(CurrentAdInfo);
                OnAdStarted?.Invoke(CurrentAdInfo);
                // NOTE: Temporary Disable Sounds

                if (bIsRewarded) {
                    AppLovin.ShowRewarded();
                } else {
                    AppLovin.ShowInterstitial();
                }

            } else {
                CurrentAdInfo = null;
                AppLovin_OnInterDismissed();
            }
        }

        private bool IntersAllowedToShow() {
            return (bIntersOn && (Time.time - LastInterShown > CooldownBetweenInterstitials));
        }

        private void RestartInterCooldown() {
            if (CooldownBetweenInterstitials > 0) {
                LastInterShown = Time.time;
            }
        }
        private static IEnumerator Ping(UnityAction<bool> callback) {
            if (callback == null) { yield break; }

            bool result;
            using (UnityWebRequest request = UnityWebRequest.Head("https://www.google.com/")) {
                request.timeout = 3;
                yield return request.SendWebRequest();
                result = request.result != UnityWebRequest.Result.ProtocolError && request.result != UnityWebRequest.Result.ConnectionError;
            }
            if (!result)
                Debug.LogWarning("Some problem with connection.");
            callback(result);
        }
        private void OnPingComplete(bool bHasInternet) {
            if (CurrentAdInfo != null) {
                CurrentAdInfo.Availability = "not_available";
                CurrentAdInfo.HasInternet = bHasInternet;
                OnAdAvailable?.Invoke(CurrentAdInfo);
            }
        }

        #endregion
    }
}
