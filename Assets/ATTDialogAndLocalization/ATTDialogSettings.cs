using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// UTUD stands for User Tracking Usage Description
[CreateAssetMenu(fileName = "ATTDialogSettings", menuName = "iOS/App Tracking Transparency Dialog Settings")]
public class ATTDialogSettings: ScriptableObject
{
    [SerializeField]
    private bool _useCustomATTDialog = false;

    [SerializeField]
    private List<string> _skAdNetworkIdentifiers = new List<string>();

    public bool UseCustomATTDialog => _useCustomATTDialog;
    public List<string> SKAdNetworkIdentifiers => _skAdNetworkIdentifiers;
}
