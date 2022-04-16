using UnityEngine;
using Zenject;

public class UISongPlatforms : UIEntity
{
	[SerializeField] GameObject m_Spotify;
	[SerializeField] GameObject m_Deezer;
	[SerializeField] GameObject m_AppleMusic;

	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] UrlProcessor       m_UrlProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;
	string m_SpotifyURL;
	string m_DeezerURL;
	string m_AppleMusicURL;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_AppleMusicURL = m_SongsProcessor.GetAppleMusicURL(m_SongID);
		m_SpotifyURL    = m_SongsProcessor.GetSpotifyURL(m_SongID);
		m_DeezerURL     = m_SongsProcessor.GetDeezerURL(m_SongID);
		
		m_Spotify.SetActive(!string.IsNullOrEmpty(m_SpotifyURL));
		m_Deezer.SetActive(!string.IsNullOrEmpty(m_DeezerURL));
		m_AppleMusic.SetActive(!string.IsNullOrEmpty(m_AppleMusicURL));
	}

	public async void OpenSpotify()
	{
		m_StatisticProcessor.LogResultMenuControlPagePlatformClick(m_SongID, m_AppleMusicURL);
		
		await m_UrlProcessor.ProcessURL(m_SpotifyURL);
	}

	public async void OpenDeezer()
	{
		m_StatisticProcessor.LogResultMenuControlPagePlatformClick(m_SongID, m_DeezerURL);
		
		await m_UrlProcessor.ProcessURL(m_DeezerURL);
	}

	public async void OpenAppleMusic()
	{
		m_StatisticProcessor.LogResultMenuControlPagePlatformClick(m_SongID, m_AppleMusicURL);
		
		await m_UrlProcessor.ProcessURL(m_AppleMusicURL);
	}
}