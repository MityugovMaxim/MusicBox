using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class LevelProcessor
{
	public string LevelID => m_LevelID;

	readonly SignalBus     m_SignalBus;
	readonly Level.Factory m_LevelFactory;

	Level  m_Level;
	string m_LevelID;

	readonly List<string>                  m_LevelIDs           = new List<string>();
	readonly Dictionary<string, LevelInfo> m_LevelInfos         = new Dictionary<string, LevelInfo>();
	readonly Dictionary<string, AudioClip> m_PreviewClips       = new Dictionary<string, AudioClip>();
	readonly Dictionary<string, Sprite>    m_PreviewBackgrounds = new Dictionary<string, Sprite>();
	readonly Dictionary<string, Sprite>    m_PreviewThumbnails  = new Dictionary<string, Sprite>();

	[Inject]
	public LevelProcessor(SignalBus _SignalBus, Level.Factory _LevelFactory)
	{
		m_SignalBus    = _SignalBus;
		m_LevelFactory = _LevelFactory;
		
		LevelRegistry levelRegistry = Resources.Load<LevelRegistry>("LevelRegistry/level_registry");
		
		foreach (LevelInfo levelInfo in levelRegistry)
		{
			m_LevelIDs.Add(levelInfo.ID);
			m_LevelInfos[levelInfo.ID] = levelInfo;
		}
	}

	public string[] GetLevelIDs()
	{
		return m_LevelIDs.ToArray();
	}

	public string GetArtist(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[LevelProcessor] Get artist failed. Level ID is null or empty.");
			return string.Empty;
		}
		
		if (!m_LevelInfos.ContainsKey(_LevelID))
		{
			Debug.LogErrorFormat("[LevelProcessor] Get artist failed. Level with ID '{0}' not found.", _LevelID);
			return string.Empty;
		}
		
		LevelInfo levelInfo = m_LevelInfos[_LevelID];
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get artist failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelInfo.Artist;
	}

	public string GetTitle(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[LevelProcessor] Get title failed. Level ID is null or empty.");
			return string.Empty;
		}
		
		if (!m_LevelInfos.ContainsKey(_LevelID))
		{
			Debug.LogErrorFormat("[LevelProcessor] Get title failed. Level with ID '{0}' not found.", _LevelID);
			return string.Empty;
		}
		
		LevelInfo levelInfo = m_LevelInfos[_LevelID];
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get title failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelInfo.Title;
	}

	public string GetNextLevelID(string _LevelID)
	{
		int index = m_LevelIDs.IndexOf(_LevelID);
		
		if (index < 0)
			return _LevelID;
		
		index = MathUtility.Repeat(index + 1, m_LevelIDs.Count);
		
		return m_LevelIDs[index];
	}

	public string GetPreviousLevelID(string _LevelID)
	{
		int index = m_LevelIDs.IndexOf(_LevelID);
		
		if (index < 0)
			return _LevelID;
		
		index = MathUtility.Repeat(index - 1, m_LevelIDs.Count);
		
		return m_LevelIDs[index];
	}

	public AudioClip GetPreviewClip(string _LevelID)
	{
		if (m_PreviewClips.ContainsKey(_LevelID) && m_PreviewClips[_LevelID] != null)
			return m_PreviewClips[_LevelID];
		
		string path = $"{_LevelID}/preview_clip";
		
		AudioClip previewClip = Resources.Load<AudioClip>(path);
		
		m_PreviewClips[_LevelID] = previewClip;
		
		return previewClip;
	}

	public Sprite GetPreviewBackground(string _LevelID)
	{
		if (m_PreviewBackgrounds.ContainsKey(_LevelID) && m_PreviewBackgrounds[_LevelID] != null)
			return m_PreviewBackgrounds[_LevelID];
		
		string path = $"{_LevelID}/preview_background";
		
		Sprite previewBackground = Resources.Load<Sprite>(path);
		
		m_PreviewBackgrounds[_LevelID] = previewBackground;
		
		return previewBackground;
	}

	public Sprite GetPreviewThumbnail(string _LevelID)
	{
		if (m_PreviewThumbnails.ContainsKey(_LevelID) && m_PreviewThumbnails[_LevelID] != null)
			return m_PreviewThumbnails[_LevelID];
		
		string path = $"{_LevelID}/preview_thumbnail";
		
		Sprite previewThumbnail = Resources.Load<Sprite>(path);
		
		m_PreviewThumbnails[_LevelID] = previewThumbnail;
		
		return previewThumbnail;
	}

	public void Create(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[LevelProcessor] Create level failed. Level ID is null or empty.");
			return;
		}
		
		if (!m_LevelInfos.ContainsKey(_LevelID))
		{
			Debug.LogErrorFormat("[LevelProcessor] Create level failed. Level with ID '{0}' not found.", _LevelID);
			return;
		}
		
		LevelInfo levelInfo = m_LevelInfos[_LevelID];
		
		if (levelInfo == null)
		{
			Debug.LogError("[LevelProvider] Create level failed. Level info is null.");
			return;
		}
		
		if (m_Level != null)
		{
			Debug.LogErrorFormat("[LevelProvider] Create level failed. Level instance '{0}' already created.", m_Level.name);
			return;
		}
		
		m_LevelFactory.Create(
			$"{levelInfo.ID}/level",
			_Level =>
			{
				m_Level   = _Level;
				m_LevelID = levelInfo.ID;
				
				m_SignalBus.Fire(new LevelStartSignal(m_LevelID));
			}
		);
	}

	public void Remove()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Remove level failed. Level is null.");
			return;
		}
		
		m_SignalBus.Fire(new LevelExitSignal(m_LevelID));
		
		GameObject.Destroy(m_Level.gameObject);
		
		m_Level   = null;
		m_LevelID = null;
	}

	public void Play()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Play level failed. Level is null.");
			return;
		}
		
		m_Level.Play(InvokeLevelComplete);
	}

	public void Pause()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Pause level failed. Level is null.");
			return;
		}
		
		m_Level.Pause();
	}

	public void Restart()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Restart level failed. Level is null.");
			return;
		}
		
		m_Level.Stop();
		
		m_SignalBus.Fire(new LevelRestartSignal(m_LevelID));
	}

	void InvokeLevelComplete()
	{
		m_SignalBus.Fire(new LevelFinishSignal(m_LevelID));
	}
}