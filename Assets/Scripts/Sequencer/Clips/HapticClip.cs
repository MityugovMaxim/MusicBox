using UnityEngine;

public class HapticClip : Clip
{
	[SerializeField] Haptic.Type m_HapticType;

	Haptic m_Haptic;

	public void Initialize(Sequencer _Sequencer, Haptic _Haptic)
	{
		base.Initialize(_Sequencer);
		
		m_Haptic = _Haptic;
	}

	protected override void OnEnter(float _Time) { }

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		m_Haptic.Process(m_HapticType);
	}
}