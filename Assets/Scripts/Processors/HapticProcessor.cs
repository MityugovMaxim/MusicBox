using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class HapticProcessor
{
	const string HAPTIC_ENABLED_KEY = "HAPTIC_ENABLED";

	public bool HapticSupported => m_Haptic != null && m_Haptic.SupportsHaptic;

	public bool HapticEnabled
	{
		get => m_HapticEnabled;
		set
		{
			if (m_HapticEnabled == value)
				return;
			
			m_HapticEnabled = value;
			
			PlayerPrefs.SetInt(HAPTIC_ENABLED_KEY, m_HapticEnabled ? 1 : 0);
		}
	}

	Haptic m_Haptic;
	bool   m_HapticEnabled;

	public HapticProcessor()
	{
		m_Haptic        = Haptic.Create();
		m_HapticEnabled = PlayerPrefs.GetInt(HAPTIC_ENABLED_KEY, 1) > 0;
	}

	public void Process(Haptic.Type _HapticType)
	{
		if (m_HapticEnabled)
			m_Haptic.Process(_HapticType);
	}

	public async void Play(Haptic.Type _HapticType, int _Frequency, float _Duration)
	{
		await UnityTask.Tick(() => Process(_HapticType), _Frequency, _Duration);
	}
}
