using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelModeButton : UIEntity
{
	[SerializeField] Button m_FreeButton;
	[SerializeField] Button m_AdsButton;
	[SerializeField] Image  m_LockIcon;

	LevelProcessor   m_LevelProcessor;
	LevelManager     m_LevelManager;

	[Inject]
	public void Construct(
		LevelProcessor _LevelProcessor,
		LevelManager   _LevelManager
	)
	{
		m_LevelProcessor = _LevelProcessor;
		m_LevelManager   = _LevelManager;
	}

	public void Setup(string _LevelID)
	{
		LevelMode levelMode      = m_LevelProcessor.GetMode(_LevelID);
		bool      levelAvailable = m_LevelManager.IsLevelAvailable(_LevelID);
		
		if (m_FreeButton != null)
			m_FreeButton.gameObject.SetActive(levelAvailable && levelMode == LevelMode.Free);
		
		if (m_AdsButton != null)
			m_AdsButton.gameObject.SetActive(levelAvailable && levelMode == LevelMode.Ads);
		
		if (m_LockIcon != null)
			m_LockIcon.gameObject.SetActive(!levelAvailable);
	}
}