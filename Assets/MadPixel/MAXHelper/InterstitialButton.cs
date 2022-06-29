using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MAXHelper {
    public class InterstitialButton : MonoBehaviour {
        #region Fields
        private Button MyButton;
        private UnityAction Callback;
        #endregion

        #region Unity Events
        private void Start() {
            MyButton = GetComponent<Button>();
            MyButton.onClick.AddListener(OnAdClick);
        }
        private void OnDestroy() {
            UnsubscribeAds();
        }
        #endregion


        public void SetCallback(UnityAction OnAdClosed) {
            Callback = OnAdClosed;
        }

        public void OnAdClick() {
            MyButton.enabled = false;
            if (!AdsManager.Instance.ShowAd(null, OnInterDismissed, "interstitial", false)) {
                Debug.LogError("Inter cant be loaded, or Cooldown between Interstitials is not over (set it in AdsManager)!");
                MyButton.enabled = true;

                // NOTE: Typically you don't show any error UI if the interstitial couldn't be loaded
                Callback?.Invoke();
            }
        }

        private bool CanShowAd() {
            if (AdsManager.Exist) {
                return AdsManager.Instance.HasLoadedAd(false);
            }
            return false;
        }

        private void OnInterDismissed() {
            Debug.Log($"User dismissed the interstitial ad");
            UnsubscribeAds();
            MyButton.enabled = true;

            Callback?.Invoke();
        }

        private void UnsubscribeAds() {
            if (AdsManager.Exist) {
                AdsManager.Instance.Subscribe(OnInterDismissed, false);
            }
        }
    }
}