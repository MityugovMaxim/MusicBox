using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelModeButton : UIEntity
{
	[SerializeField] Button m_FreeButton;
	[SerializeField] Button m_AdsButton;
	[SerializeField] Image  m_LockIcon;

	LevelProcessor   m_LevelProcessor;
	ProfileProcessor m_ProfileProcessor;

	[Inject]
	public void Construct(LevelProcessor _LevelProcessor, ProfileProcessor _ProfileProcessor)
	{
		m_LevelProcessor   = _LevelProcessor;
		m_ProfileProcessor = _ProfileProcessor;
	}

	public void Setup(string _LevelID)
	{
		LevelMode levelMode      = m_ProfileProcessor.HasNoAds() ? LevelMode.Free : m_LevelProcessor.GetMode(_LevelID);
		bool      levelAvailable = m_ProfileProcessor.HasLevel(_LevelID);
		
		if (m_FreeButton != null)
			m_FreeButton.gameObject.SetActive(levelAvailable && levelMode == LevelMode.Free);
		
		if (m_AdsButton != null)
			m_AdsButton.gameObject.SetActive(levelAvailable && levelMode == LevelMode.Ads);
		
		if (m_LockIcon != null)
			m_LockIcon.gameObject.SetActive(!levelAvailable);
	}
}