using UnityEngine;
using Zenject;

public class UISongMode : UIEntity
{
	[SerializeField] GameObject m_FreeMode;
	[SerializeField] GameObject m_AdsMode;
	[SerializeField] GameObject m_ProductMode;
	[SerializeField] GameObject m_Lock;

	[Inject] SongsProcessor m_SongsProcessor;
	[Inject] SongsManager   m_SongsManager;

	public void Setup(string _LevelID)
	{
		LevelMode levelMode      = m_SongsProcessor.GetMode(_LevelID);
		bool      levelAvailable = m_SongsManager.IsSongAvailable(_LevelID);
		
		m_FreeMode.SetActive(levelAvailable && levelMode == LevelMode.Free);
		m_AdsMode.SetActive(levelAvailable && levelMode == LevelMode.Ads);
		m_ProductMode.SetActive(levelAvailable && levelMode == LevelMode.Product);
		
		m_Lock.SetActive(!levelAvailable);
	}
}