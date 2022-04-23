#if UNITY_ANDROID
using UnityEngine;

public class AndroidHaptic : Haptic
{
	const string CLASS_NAME = "com.audiobox.hapticcontroller.HapticController";

	public override bool SupportsHaptic => true;

	AndroidJavaObject m_HapticController;

	protected override void Initialize()
	{
		if (!SupportsHaptic)
			return;
		
		m_HapticController = new AndroidJavaObject(CLASS_NAME);
		m_HapticController.Call("Initialize");
	}

	public override void Process(Type _Type)
	{
		if (!SupportsHaptic)
			return;
		
		switch (_Type)
		{
			case Type.Selection:
				m_HapticController.Call("Selection");
				break;
			case Type.Success:
				m_HapticController.Call("Success");
				break;
			case Type.Warning:
				m_HapticController.Call("Warning");
				break;
			case Type.Failure:
				m_HapticController.Call("Failure");
				break;
			case Type.ImpactLight:
				m_HapticController.Call("ImpactLight");
				break;
			case Type.ImpactMedium:
				m_HapticController.Call("ImpactMedium");
				break;
			case Type.ImpactHeavy:
				m_HapticController.Call("ImpactHeavy");
				break;
			case Type.ImpactRigid:
				m_HapticController.Call("ImpactRigid");
				break;
			case Type.ImpactSoft:
				m_HapticController.Call("ImpactSoft");
				break;
		}
	}
}
#endif