using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIProductSongItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductSongItem> { }

	public string SongID { get; private set; }

	[SerializeField] UISongImage m_Image;
	[SerializeField] Button      m_PlayButton;
	[SerializeField] Button      m_PauseButton;

	Action<string> m_Play;
	Action<string> m_Stop;
	bool           m_Playing;

	public void Setup(string _SongID, Action<string> _Play, Action<string> _Stop)
	{
		SongID = _SongID;
		
		m_Play    = _Play;
		m_Stop    = _Stop;
		m_Playing = false;
		
		m_PlayButton.gameObject.SetActive(true);
		m_PauseButton.gameObject.SetActive(false);
		
		m_PlayButton.onClick.RemoveAllListeners();
		m_PlayButton.onClick.AddListener(Play);
		
		m_PauseButton.onClick.RemoveAllListeners();
		m_PauseButton.onClick.AddListener(Stop);
		
		m_Image.Setup(_SongID);
	}

	[Preserve]
	public void Play()
	{
		if (m_Playing)
			return;
		
		m_Playing = true;
		
		m_PlayButton.gameObject.SetActive(false);
		m_PauseButton.gameObject.SetActive(true);
		
		m_Play?.Invoke(SongID);
	}

	[Preserve]
	public void Stop()
	{
		if (!m_Playing)
			return;
		
		m_Playing = false;
		
		m_PlayButton.gameObject.SetActive(true);
		m_PauseButton.gameObject.SetActive(false);
		
		m_Stop?.Invoke(SongID);
	}
}