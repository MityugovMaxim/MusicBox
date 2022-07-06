using UnityEngine;
using Zenject;

public class UISongMode : UIEntity
{
	[SerializeField] GameObject m_FreeMode;
	[SerializeField] GameObject m_AdsMode;
	[SerializeField] GameObject m_ProductMode;
	[SerializeField] UILevel    m_Level;

	[Inject] SongsProcessor    m_SongsProcessor;
	[Inject] SongsManager      m_SongsManager;
	[Inject] ProgressProcessor m_ProgressProcessor;

	public void Setup(string _SongID)
	{
		SongMode songMode      = m_SongsProcessor.GetMode(_SongID);
		bool     songAvailable = m_SongsManager.IsSongAvailable(_SongID);
		
		m_FreeMode.SetActive(songAvailable && songMode == SongMode.Free);
		m_AdsMode.SetActive(songAvailable && songMode == SongMode.Ads);
		m_ProductMode.SetActive(songAvailable && songMode == SongMode.Paid);
		
		m_Level.Level = m_ProgressProcessor.GetSongLevel(_SongID);
		m_Level.gameObject.SetActive(!songAvailable && songMode != SongMode.Paid);
	}
}