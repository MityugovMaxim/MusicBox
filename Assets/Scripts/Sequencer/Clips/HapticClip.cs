using UnityEngine;

public class HapticClip : Clip
{
	[SerializeField] Haptic.Type m_HapticType;

	HapticProcessor m_HapticProcessor;

	public void Initialize(Sequencer _Sequencer, HapticProcessor _HapticProcessor)
	{
		base.Initialize(_Sequencer);
		
		m_HapticProcessor = _HapticProcessor;
	}

	protected override void OnEnter(float _Time) { }

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		m_HapticProcessor.Process(m_HapticType);
	}
}