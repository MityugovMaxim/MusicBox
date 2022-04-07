using UnityEngine;
using Zenject;

public class UISongPlatforms : UIEntity
{
	const string APPLE_MUSIC_PLATFORM_ID = "apple_music";
	const string SPOTIFY_PLATFORM_ID     = "spotify";
	const string DEEZER_PLATFORM_ID      = "deezer";

	[SerializeField] GameObject m_Spotify;
	[SerializeField] GameObject m_Deezer;
	[SerializeField] GameObject m_AppleMusic;

	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] UrlProcessor       m_UrlProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;

	string m_LevelID;
	string m_SpotifyURL;
	string m_DeezerURL;
	string m_AppleMusicURL;

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_SpotifyURL    = m_SongsProcessor.GetPlatformURL(m_LevelID, SPOTIFY_PLATFORM_ID);
		m_DeezerURL     = m_SongsProcessor.GetPlatformURL(m_LevelID, DEEZER_PLATFORM_ID);
		m_AppleMusicURL = m_SongsProcessor.GetPlatformURL(m_LevelID, APPLE_MUSIC_PLATFORM_ID);
		
		m_Spotify.SetActive(!string.IsNullOrEmpty(m_SpotifyURL));
		m_Deezer.SetActive(!string.IsNullOrEmpty(m_DeezerURL));
		m_AppleMusic.SetActive(!string.IsNullOrEmpty(m_AppleMusicURL));
	}

	public async void OpenSpotify()
	{
		m_StatisticProcessor.LogResultMenuControlPagePlatformClick(m_LevelID, SPOTIFY_PLATFORM_ID);
		
		await m_UrlProcessor.ProcessURL(m_SpotifyURL);
	}

	public async void OpenDeezer()
	{
		m_StatisticProcessor.LogResultMenuControlPagePlatformClick(m_LevelID, DEEZER_PLATFORM_ID);
		
		await m_UrlProcessor.ProcessURL(m_DeezerURL);
	}

	public async void OpenAppleMusic()
	{
		m_StatisticProcessor.LogResultMenuControlPagePlatformClick(m_LevelID, APPLE_MUSIC_PLATFORM_ID);
		
		await m_UrlProcessor.ProcessURL(m_AppleMusicURL);
	}
}