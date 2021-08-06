using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelModeButton : UIEntity
{
	[SerializeField] Button m_FreeButton;
	[SerializeField] Button m_AdsButton;

	LevelProcessor m_LevelProcessor;

	[Inject]
	public void Construct(LevelProcessor _LevelProcessor)
	{
		m_LevelProcessor = _LevelProcessor;
	}

	public void Setup(string _LevelID)
	{
		LevelMode levelMode = m_LevelProcessor.GetLevelMode(_LevelID);
		
		if (m_FreeButton != null)
			m_FreeButton.gameObject.SetActive(levelMode == LevelMode.Free);
		
		if (m_AdsButton != null)
			m_AdsButton.gameObject.SetActive(levelMode == LevelMode.Ads);
	}
}