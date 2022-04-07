using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsManager
{
	[Inject] ProfileProcessor  m_ProfileProcessor;
	[Inject] SongsProcessor    m_SongsProcessor;
	[Inject] ScoreProcessor    m_ScoreProcessor;
	[Inject] ProductsProcessor m_ProductsProcessor;

	public List<string> GetLibrarySongIDs()
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(_SongID => m_ProfileProcessor.HasSong(_SongID) || IsSongLockedByCoins(_SongID) || !IsSongLockedByLevel(_SongID))
			.OrderByDescending(m_SongsProcessor.GetBadge)
			.ThenByDescending(m_SongsProcessor.GetLevel)
			.ThenBy(m_ScoreProcessor.GetRank)
			.ThenBy(m_SongsProcessor.GetPrice)
			.ToList();
	}

	public List<string> GetProductSongIDs()
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(IsSongLockedByProduct)
			.OrderByDescending(m_SongsProcessor.GetBadge)
			.ToList();
	}

	public Dictionary<int, string[]> GetLockedSongIDs()
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(IsSongLockedByLevel)
			.GroupBy(m_SongsProcessor.GetLevel)
			.OrderBy(_LevelIDs => _LevelIDs.Key)
			.ToDictionary(_LevelIDs => _LevelIDs.Key, _LevelIDs => _LevelIDs.ToArray());
	}

	public List<string> GetLockedSongIDs(int _Level)
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(IsSongLockedByLevel)
			.Where(_LevelID => _Level == m_SongsProcessor.GetLevel(_LevelID))
			.ToList();
	}

	public bool IsSongLockedByProduct(string _SongID)
	{
		if (m_ProfileProcessor.HasSong(_SongID))
			return false;
		
		return false;
	}

	public bool IsSongLockedByLevel(string _SongID)
	{
		if (m_ProfileProcessor.HasSong(_SongID))
			return false;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_SongsProcessor.GetLevel(_SongID);
		
		return currentLevel < requiredLevel;
	}

	public bool IsSongLockedByCoins(string _SongID)
	{
		if (m_ProfileProcessor.HasSong(_SongID))
			return false;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_SongsProcessor.GetLevel(_SongID);
		
		if (currentLevel < requiredLevel)
			return false;
		
		return m_SongsProcessor.GetPrice(_SongID) > 0;
	}

	public bool IsSongAvailable(string _SongID)
	{
		if (m_ProfileProcessor.HasSong(_SongID))
			return true;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_SongsProcessor.GetLevel(_SongID);
		
		if (currentLevel < requiredLevel)
			return false;
		
		return m_SongsProcessor.GetPrice(_SongID) == 0;
	}
}