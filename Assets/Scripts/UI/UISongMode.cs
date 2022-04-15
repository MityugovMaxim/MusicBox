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
		SongMode songMode      = m_SongsProcessor.GetMode(_LevelID);
		bool     songAvailable = m_SongsManager.IsSongAvailable(_LevelID);
		
		m_FreeMode.SetActive(songAvailable && songMode == SongMode.Free);
		m_AdsMode.SetActive(songAvailable && songMode == SongMode.Ads);
		m_ProductMode.SetActive(songAvailable && songMode == SongMode.Product);
		
		m_Lock.SetActive(!songAvailable);
	}
}