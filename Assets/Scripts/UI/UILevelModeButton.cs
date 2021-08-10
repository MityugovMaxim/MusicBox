using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelModeButton : UIEntity
{
	[SerializeField] Button m_FreeButton;
	[SerializeField] Button m_AdsButton;
	[SerializeField] Image  m_LockIcon;

	LevelProcessor    m_LevelProcessor;
	ProgressProcessor m_ProgressProcessor;

	[Inject]
	public void Construct(LevelProcessor _LevelProcessor, ProgressProcessor _ProgressProcessor)
	{
		m_LevelProcessor    = _LevelProcessor;
		m_ProgressProcessor = _ProgressProcessor;
	}

	public void Setup(string _LevelID)
	{
		LevelMode levelMode     = m_LevelProcessor.GetLevelMode(_LevelID);
		bool      levelUnlocked = m_ProgressProcessor.IsLevelUnlocked(_LevelID);
		
		if (m_FreeButton != null)
			m_FreeButton.gameObject.SetActive(levelUnlocked && levelMode == LevelMode.Free);
		
		if (m_AdsButton != null)
			m_AdsButton.gameObject.SetActive(levelUnlocked && levelMode == LevelMode.Ads);
		
		if (m_LockIcon != null)
			m_LockIcon.gameObject.SetActive(!levelUnlocked);
	}
}