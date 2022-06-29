using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MAXHelper {
    public class AdButton : MonoBehaviour {
        #region Fields
        [SerializeField] private string Placement = "revive_hero";
        private Button MyButton;
        private UnityAction<bool> Callback;
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


        #region Public
        public void OnAdClick() {
            MyButton.enabled = false;
            if (!AdsManager.Instance.ShowAd(OnFinishAds, OnAdCancelled, Placement)) {
                Debug.LogError("Ad is not ready yet!");
                MyButton.enabled = true;

                // NOTE: You can show your UI Error Panel here
                // or add a negative result Callback:
                Callback?.Invoke(false);
            }
        }

        public void SetCallback(UnityAction<bool> OnAdClosed) {
            Callback = OnAdClosed;
        }
        #endregion

        #region Helpers
        private bool CanShowAd() {
            if (AdsManager.Exist) {
                return AdsManager.Instance.HasLoadedAd();
            }
            return false;
        }

        private void OnFinishAds(bool Success) {
            if (Success) {
                Debug.Log($"Give reward to user!");
                Callback?.Invoke(true);

                UnsubscribeAds();
                MyButton.enabled = true;
            } else {
                OnAdCancelled();
            }
        }

        private void OnAdCancelled() {
            Debug.Log($"User closed rewarded ad");
            UnsubscribeAds();
            MyButton.enabled = true;

            Callback?.Invoke(false);
        }

        private void UnsubscribeAds() {
            if (AdsManager.Exist) {
                AdsManager.Instance.Subscribe(OnFinishAds, OnAdCancelled, false);
            }
        } 
        #endregion
    }
}