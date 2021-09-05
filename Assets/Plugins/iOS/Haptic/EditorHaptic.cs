using UnityEngine;

public class EditorHaptic : Haptic
{
	public override bool SupportsHaptic => false;

	protected override void Initialize() { }

	public override void Process(Type _Type)
	{
		Debug.LogFormat("[EditorHaptic] Haptic: {0}", _Type);
	}
}