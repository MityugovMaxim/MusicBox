using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Zenject.Internal;

public class UIProductMenuItem : UIEntity
{
	public string LevelID { get; private set; }

	[UnityEngine.Scripting.Preserve]
	public class Factory : PlaceholderFactory<UIProductMenuItem, UIProductMenuItem> { }

	[SerializeField] UILevelPreviewThumbnail m_Thumbnail;
	[SerializeField] Button                  m_PlayButton;
	[SerializeField] Button                  m_PauseButton;

	Action<string> m_Play;
	Action<string> m_Stop;
	bool           m_Playing;

	public void Setup(string _LevelID, Action<string> _Play, Action<string> _Stop)
	{
		LevelID = _LevelID;
		
		m_Play    = _Play;
		m_Stop    = _Stop;
		m_Playing = false;
		
		m_PlayButton.gameObject.SetActive(true);
		m_PauseButton.gameObject.SetActive(false);
		
		m_Thumbnail.Setup(_LevelID);
	}

	[Preserve]
	public void Play()
	{
		if (m_Playing)
			return;
		
		m_Playing = true;
		
		m_PlayButton.gameObject.SetActive(false);
		m_PauseButton.gameObject.SetActive(true);
		
		m_Play?.Invoke(LevelID);
	}

	[Preserve]
	public void Stop()
	{
		if (!m_Playing)
			return;
		
		m_Playing = false;
		
		m_PlayButton.gameObject.SetActive(true);
		m_PauseButton.gameObject.SetActive(false);
		
		m_Stop?.Invoke(LevelID);
	}
}