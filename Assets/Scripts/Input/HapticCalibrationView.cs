using TMPro;
using UnityEngine;

public class HapticCalibrationView : MonoBehaviour
{
	public static float HapticOffset
	{
		get => PlayerPrefs.GetFloat("HAPTIC_OFFSET", 0);
		set => PlayerPrefs.SetFloat("HAPTIC_OFFSET", value);
	}

	[SerializeField] TMP_Text m_Text;

	void Awake()
	{
		m_Text.text = HapticOffset.ToString("F2");
	}

	public void AddOffset()
	{
		HapticOffset += 0.01f;
		
		m_Text.text = HapticOffset.ToString("F2");
	}

	public void RemoveOffset()
	{
		HapticOffset -= 0.01f;
		
		m_Text.text = HapticOffset.ToString("F2");
	}
}