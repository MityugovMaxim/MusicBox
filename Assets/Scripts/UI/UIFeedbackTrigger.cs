using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UIFeedbackTrigger : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	[Header("Haptic")]
	[SerializeField] Haptic.Type m_TouchDownHaptic;
	[SerializeField] Haptic.Type m_TouchUpHaptic;
	[SerializeField] Haptic.Type m_TouchClickHaptic;

	[Header("Sound")]
	[SerializeField, Sound] string m_TouchDownSound;
	[SerializeField, Sound] string m_TouchUpSound;
	[SerializeField, Sound] string m_TouchClickSound;

	[Inject] HapticProcessor m_HapticProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;

	[SerializeField] Button m_Button;

	void PlayHaptic(Haptic.Type _Type)
	{
		if (_Type == Haptic.Type.None)
			return;
		
		if (m_Button == null || m_Button.interactable)
			m_HapticProcessor.Process(_Type);
	}

	void PlaySound(string _SoundID)
	{
		if (string.IsNullOrEmpty(_SoundID))
			return;
		
		if (m_Button == null || m_Button.interactable)
			m_SoundProcessor.Play(_SoundID);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		PlayHaptic(m_TouchDownHaptic);
		PlaySound(m_TouchDownSound);
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		PlayHaptic(m_TouchUpHaptic);
		PlaySound(m_TouchUpSound);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		PlayHaptic(m_TouchClickHaptic);
		PlaySound(m_TouchClickSound);
	}
}