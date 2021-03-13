using UnityEngine;

public class HapticClip : Clip
{
	[SerializeField] Haptic.Type m_HapticType;

	protected override void OnEnter(float _Time) { }

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		Haptic.Process(m_HapticType);
	}

	protected override void OnStop(float _Time) { }
}