using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class LevelProcessor
{
	Level  m_Level;
	string m_LevelID;

	readonly SignalBus                     m_SignalBus;
	readonly PurchaseProcessor             m_PurchaseProcessor;
	readonly ProgressProcessor             m_ProgressProcessor;
	readonly Level.Factory                 m_LevelFactory;
	readonly ProductInfo                   m_NoAdsProduct;
	readonly List<string>                  m_LevelIDs           = new List<string>();
	readonly Dictionary<string, LevelInfo> m_LevelInfos         = new Dictionary<string, LevelInfo>();
	readonly Dictionary<string, AudioClip> m_PreviewClips       = new Dictionary<string, AudioClip>();
	readonly Dictionary<string, Sprite>    m_PreviewBackgrounds = new Dictionary<string, Sprite>();
	readonly Dictionary<string, Sprite>    m_PreviewThumbnails  = new Dictionary<string, Sprite>();
	readonly List<ISampleReceiver>         m_SampleReceivers    = new List<ISampleReceiver>();

	[Inject]
	public LevelProcessor(
		SignalBus         _SignalBus,
		PurchaseProcessor _PurchaseProcessor,
		ProgressProcessor _ProgressProcessor,
		Level.Factory     _LevelFactory,
		ProductInfo       _NoAdsProduct 
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_LevelFactory      = _LevelFactory;
		m_NoAdsProduct      = _NoAdsProduct;
		
		LevelRegistry levelRegistry = Registry.Load<LevelRegistry>("level_registry");
		if (levelRegistry != null)
		{
			foreach (LevelInfo levelInfo in levelRegistry)
			{
				if (levelInfo == null || !levelInfo.Active)
					continue;
				
				m_LevelIDs.Add(levelInfo.ID);
				m_LevelInfos[levelInfo.ID] = levelInfo;
			}
		}
	}

	public string[] GetLevelIDs()
	{
		return m_LevelIDs.Where(_LevelID => m_PurchaseProcessor.IsLevelPurchased(_LevelID))
			.OrderByDescending(m_ProgressProcessor.IsLevelUnlocked)
			.ThenBy(m_ProgressProcessor.GetExpRequired)
			.ToArray();
	}

	public bool Contains(string _LevelID)
	{
		return m_LevelInfos.ContainsKey(_LevelID);
	}

	public string GetArtist(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get artist failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelInfo.Artist;
	}

	public string GetTitle(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get title failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelInfo.Title;
	}

	public string GetLeaderboardID(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get leaderboard ID failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelInfo.LeaderboardID;
	}

	public string GetAchievementID(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get achievement ID failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelInfo.AchievementID;
	}

	public string GetNextLevelID(string _LevelID)
	{
		int index = m_LevelIDs.IndexOf(_LevelID);
		
		if (index < 0)
			return _LevelID;
		
		for (int i = 1; i <= m_LevelIDs.Count; i++)
		{
			int j = MathUtility.Repeat(index + i, m_LevelIDs.Count);
			
			string levelID = m_LevelIDs[j];
			
			if (!m_PurchaseProcessor.IsLevelPurchased(levelID) || m_ProgressProcessor.IsLevelLocked(levelID))
				continue;
			
			return levelID;
		}
		
		return _LevelID;
	}

	public string GetPreviousLevelID(string _LevelID)
	{
		int index = m_LevelIDs.IndexOf(_LevelID);
		
		if (index < 0)
			return _LevelID;
		
		for (int i = 1; i <= m_LevelIDs.Count; i++)
		{
			int j = MathUtility.Repeat(index - i, m_LevelIDs.Count);
			
			string levelID = m_LevelIDs[j];
			
			if (!m_PurchaseProcessor.IsLevelPurchased(levelID) || m_ProgressProcessor.IsLevelLocked(levelID))
				continue;
			
			return levelID;
		}
		
		return _LevelID;
	}

	public LevelMode GetLevelMode(string _LevelID)
	{
		if (m_NoAdsProduct != null && m_PurchaseProcessor.IsProductPurchased(m_NoAdsProduct.ID))
			return LevelMode.Free;
		
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get mode failed. Level info with ID '{0}' is null.", _LevelID);
			return LevelMode.Free;
		}
		
		return levelInfo.Mode;
	}

	public AudioClip GetPreviewClip(string _LevelID)
	{
		if (m_PreviewClips.ContainsKey(_LevelID) && m_PreviewClips[_LevelID] != null)
			return m_PreviewClips[_LevelID];
		
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get preview clip failed. Level info not found for level with ID '{0}'.", _LevelID);
			return null;
		}
		
		if (string.IsNullOrEmpty(levelInfo.Clip))
		{
			Debug.LogErrorFormat("[LevelProcessor] Get preview clip failed. Clip is null for level with ID '{0}'.", _LevelID);
			return null;
		}
		
		AudioClip previewClip = Resources.Load<AudioClip>(levelInfo.Clip);
		
		m_PreviewClips[_LevelID] = previewClip;
		
		return previewClip;
	}

	public Sprite GetPreviewBackground(string _LevelID)
	{
		if (m_PreviewBackgrounds.ContainsKey(_LevelID) && m_PreviewBackgrounds[_LevelID] != null)
			return m_PreviewBackgrounds[_LevelID];
		
		Sprite previewThumbnail = GetPreviewThumbnail(_LevelID);
		
		if (previewThumbnail == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get preview background failed. Preview thumbnail is null for level with ID '{0}'.", _LevelID);
			return null;
		}
		
		Sprite previewBackground = BlurUtility.Blur(previewThumbnail, 0.5f, 8);
		
		m_PreviewBackgrounds[_LevelID] = previewBackground;
		
		return previewBackground;
	}

	public Sprite GetPreviewThumbnail(string _LevelID)
	{
		if (m_PreviewThumbnails.ContainsKey(_LevelID) && m_PreviewThumbnails[_LevelID] != null)
			return m_PreviewThumbnails[_LevelID];
		
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get preview thumbnail failed. Level info not found for level with ID '{0}'.", _LevelID);
			return null;
		}
		
		if (string.IsNullOrEmpty(levelInfo.Thumbnail))
		{
			Debug.LogErrorFormat("[LevelProcessor] Get preview thumbnail failed. Thumbnail is null for level with ID '{0}'.", _LevelID);
			return null;
		}
		
		Sprite previewThumbnail = Resources.Load<Sprite>(levelInfo.Thumbnail);
		
		m_PreviewThumbnails[_LevelID] = previewThumbnail;
		
		return previewThumbnail;
	}

	public void Create(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
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
		
		m_Level.Play(
			m_SampleReceivers.ToArray(),
			() => m_SignalBus.Fire(new LevelFinishSignal(m_LevelID))
		);
		
		m_SignalBus.Fire(new LevelPlaySignal(m_LevelID));
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

	public void AddSampleReceiver(ISampleReceiver _SampleReceiver)
	{
		m_SampleReceivers.Add(_SampleReceiver);
	}

	public void RemoveSampleReceiver(ISampleReceiver _SampleReceiver)
	{
		m_SampleReceivers.Remove(_SampleReceiver);
	}

	LevelInfo GetLevelInfo(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ProgressProcessor] Get level info failed. Level ID is null or empty.");
			return null;
		}
		
		if (!m_LevelInfos.ContainsKey(_LevelID))
		{
			Debug.LogErrorFormat("[ProgressProcessor] Get level info failed. Level with ID '{0}' not found.", _LevelID);
			return null;
		}
		
		return m_LevelInfos[_LevelID];
	}
}