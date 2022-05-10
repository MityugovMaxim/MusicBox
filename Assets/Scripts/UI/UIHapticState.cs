using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIHapticState : UIEntity
{
	[SerializeField] Button m_EnableButton;
	[SerializeField] Button m_DisableButton;

	StatisticProcessor m_StatisticProcessor;
	HapticProcessor    m_HapticProcessor;

	bool m_Enabled;
	bool m_State;

	[Inject]
	public void Construct(
		StatisticProcessor _StatisticProcessor,
		HapticProcessor    _HapticProcessor
	)
	{
		m_StatisticProcessor = _StatisticProcessor;
		m_HapticProcessor    = _HapticProcessor;
	}

	public void Setup()
	{
		gameObject.SetActive(m_HapticProcessor.HapticSupported);
		
		m_Enabled = m_HapticProcessor.HapticEnabled;
		m_State   = m_Enabled;
		
		if (m_EnableButton != null)
			m_EnableButton.gameObject.SetActive(!m_State);
		if (m_DisableButton != null)
			m_DisableButton.gameObject.SetActive(m_State);
	}

	public void Execute()
	{
		if (m_State == m_Enabled)
			return;
		
		m_State = m_Enabled;
		
		m_HapticProcessor.HapticEnabled = m_Enabled;
		
		m_StatisticProcessor.LogPauseMenuHaptic(m_Enabled);
	}

	public void EnableHaptic()
	{
		if (m_Enabled)
			return;
		
		m_Enabled = true;
		
		m_HapticProcessor.HapticEnabled = true;
		
		m_HapticProcessor.Process(Haptic.Type.Success);
		
		m_EnableButton.gameObject.SetActive(false);
		m_DisableButton.gameObject.SetActive(true);
	}

	public void DisableHaptic()
	{
		if (!m_Enabled)
			return;
		
		m_Enabled = false;
		
		m_HapticProcessor.HapticEnabled = false;
		
		m_EnableButton.gameObject.SetActive(true);
		m_DisableButton.gameObject.SetActive(false);
	}
}