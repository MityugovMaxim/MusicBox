using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using AOT;

public enum ATTManagerAuthorizationStatus
{
    NotDetermined = 0,
    Authorised,
    Denied,
    Restricted
}

public class ATTDialog : MonoBehaviour
{

    private static Action<ATTManagerAuthorizationStatus> _onATTComplete;

    private delegate void ATTCompletition(int status);

    public static void CallATTrackingDialog(Action<ATTManagerAuthorizationStatus> onComplete)
    {
#if UNITY_IOS
        _onATTComplete = onComplete;
        AdapterCallATTDialog(ATTCompleteCallback);
#else
        onComplete?.Invoke(ATTManagerAuthorizationStatus.Authorised);
#endif
    }

    [DllImport("__Internal")]
    private static extern void AdapterCallATTDialog(ATTCompletition onATTComplete);

    [MonoPInvokeCallback(typeof(ATTCompletition))]
    private static void ATTCompleteCallback(int status)
    {
        switch (status)
        {
            case 0:
                _onATTComplete?.Invoke(ATTManagerAuthorizationStatus.NotDetermined);
                break;
            case 1:
                _onATTComplete?.Invoke(ATTManagerAuthorizationStatus.Authorised);
                break;
            case 2:
                _onATTComplete?.Invoke(ATTManagerAuthorizationStatus.Denied);
                break;
            case 3:
                _onATTComplete?.Invoke(ATTManagerAuthorizationStatus.Restricted);
                break;
            default:
                _onATTComplete?.Invoke(ATTManagerAuthorizationStatus.Denied);
                break;
        }

        _onATTComplete = null;
    }
}
