#if UNITY_IOS && !UNITY_EDITOR

using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.iOS;

public class iOSHaptic : Haptic
{
	[DllImport("__Internal")]
	static extern void InitializeHapticGenerators();

	[DllImport("__Internal")]
	static extern void HapticSelection();

	[DllImport("__Internal")]
	static extern void HapticSuccess();

	[DllImport("__Internal")]
	static extern void HapticWarning();

	[DllImport("__Internal")]
	static extern void HapticFailure();

	[DllImport("__Internal")]
	static extern void HapticLightImpact();

	[DllImport("__Internal")]
	static extern void HapticMediumImpact();

	[DllImport("__Internal")]
	static extern void HapticHeavyImpact();

	static bool SupportsHaptic => Device.generation >= DeviceGeneration.iPhone7;

	protected override void InitializeInternal()
	{
		if (!SupportsHaptic)
			return;
		
		InitializeHapticGenerators();
	}

	protected override void ProcessInternal(Type _Type)
	{
		if (!SupportsHaptic)
			return;
		
		switch (_Type)
		{
			case Type.Default:
				Handheld.Vibrate();
				break;
			case Type.Selection:
				HapticSelection();
				return;
			case Type.Success:
				HapticSuccess();
				return;
			case Type.Warning:
				HapticWarning();
				return;
			case Type.Failure:
				HapticFailure();
				return;
			case Type.ImpactLight:
				HapticLightImpact();
				return;
			case Type.ImpactMedium:
				HapticMediumImpact();
				return;
			case Type.ImpactHeavy:
				HapticHeavyImpact();
				return;
		}
	}
}
#endif