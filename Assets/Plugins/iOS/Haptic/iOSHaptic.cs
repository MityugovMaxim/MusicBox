#if UNITY_IOS
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

	[DllImport("__Internal")]
	static extern void HapticRigidImpact();

	[DllImport("__Internal")]
	static extern void HapticSoftImpact();

	public override bool SupportsHaptic => Device.generation >= DeviceGeneration.iPhone7;

	protected override void Initialize()
	{
		if (!SupportsHaptic)
			return;
		
		InitializeHapticGenerators();
	}

	public override void Process(Type _Type)
	{
		if (!SupportsHaptic)
			return;
		
		switch (_Type)
		{
			case Type.Selection:
				HapticSelection();
				break;
			case Type.Success:
				HapticSuccess();
				break;
			case Type.Warning:
				HapticWarning();
				break;
			case Type.Failure:
				HapticFailure();
				break;
			case Type.ImpactLight:
				HapticLightImpact();
				break;
			case Type.ImpactMedium:
				HapticMediumImpact();
				break;
			case Type.ImpactHeavy:
				HapticHeavyImpact();
				break;
			case Type.ImpactRigid:
				HapticRigidImpact();
				break;
			case Type.ImpactSoft:
				HapticSoftImpact();
				break;
		}
	}
}
#endif