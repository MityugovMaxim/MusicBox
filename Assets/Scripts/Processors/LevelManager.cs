using System.Collections.Generic;
using System.Linq;
using Zenject;

public class LevelManager
{
	readonly ProfileProcessor m_ProfileProcessor;
	readonly LevelProcessor   m_LevelProcessor;
	readonly ScoreProcessor   m_ScoreProcessor;
	readonly ProductProcessor m_ProductProcessor;

	[Inject]
	public LevelManager(
		ProfileProcessor _ProfileProcessor,
		LevelProcessor   _LevelProcessor,
		ScoreProcessor   _ScoreProcessor,
		ProductProcessor _ProductProcessor
	)
	{
		m_ProfileProcessor = _ProfileProcessor;
		m_LevelProcessor   = _LevelProcessor;
		m_ScoreProcessor   = _ScoreProcessor;
		m_ProductProcessor = _ProductProcessor;
	}

	public List<string> GetLibraryLevelIDs()
	{
		return m_LevelProcessor.GetLevelIDs()
			.Where(_LevelID => m_ProfileProcessor.HasLevel(_LevelID) || IsLevelLockedByCoins(_LevelID) || !IsLevelLockedByLevel(_LevelID))
			.OrderByDescending(m_LevelProcessor.GetBadge)
			.ThenByDescending(m_LevelProcessor.GetLevel)
			.ThenBy(m_ScoreProcessor.GetRank)
			.ThenBy(m_LevelProcessor.GetPrice)
			.ToList();
	}

	public List<string> GetProductLevelIDs()
	{
		return m_LevelProcessor.GetLevelIDs()
			.Where(_LevelID => !m_ProfileProcessor.HasLevel(_LevelID))
			.Where(_LevelID => m_ProductProcessor.HasLevel(_LevelID))
			.OrderByDescending(m_LevelProcessor.GetBadge)
			.ToList();
	}

	public Dictionary<int, string[]> GetLockedLevelIDs()
	{
		return m_LevelProcessor.GetLevelIDs()
			.Where(IsLevelLockedByLevel)
			.GroupBy(m_LevelProcessor.GetLevel)
			.OrderBy(_LevelIDs => _LevelIDs.Key)
			.ToDictionary(_LevelIDs => _LevelIDs.Key, _LevelIDs => _LevelIDs.ToArray());
	}

	public List<string> GetLockedLevelIDs(int _Level)
	{
		return m_LevelProcessor.GetLevelIDs()
			.Where(IsLevelLockedByLevel)
			.Where(_LevelID => _Level == m_LevelProcessor.GetLevel(_LevelID))
			.ToList();
	}

	public bool IsLevelLockedByCoins(string _LevelID)
	{
		if (m_ProfileProcessor.HasLevel(_LevelID))
			return false;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_LevelProcessor.GetLevel(_LevelID);
		
		if (currentLevel < requiredLevel)
			return false;
		
		return m_LevelProcessor.GetPrice(_LevelID) > 0;
	}

	public bool IsLevelLockedByLevel(string _LevelID)
	{
		if (m_ProfileProcessor.HasLevel(_LevelID))
			return false;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_LevelProcessor.GetLevel(_LevelID);
		
		return currentLevel < requiredLevel;
	}

	public bool IsLevelAvailable(string _LevelID)
	{
		if (m_ProfileProcessor.HasLevel(_LevelID))
			return true;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_LevelProcessor.GetLevel(_LevelID);
		
		if (currentLevel < requiredLevel)
			return false;
		
		return m_LevelProcessor.GetPrice(_LevelID) == 0;
	}
}