using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsManager
{
	[Inject] ProfileProcessor  m_ProfileProcessor;
	[Inject] SongsProcessor    m_SongsProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] ScoresProcessor   m_ScoresProcessor;

	public List<string> GetLibrarySongIDs()
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(IsSongAvailable)
			.OrderBy(m_ScoresProcessor.GetRank)
			.ThenByDescending(m_SongsProcessor.GetBadge)
			.ThenByDescending(m_SongsProcessor.GetPrice)
			.ThenByDescending(m_ProgressProcessor.GetSongLevel)
			.ToList();
	}

	public List<string> GetCoinsSongIDs()
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(IsSongLockedByCoins)
			.OrderBy(m_SongsProcessor.GetPrice)
			.ToList();
	}

	public Dictionary<int, string[]> GetLockedSongIDs()
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(IsSongLockedByLevel)
			.GroupBy(m_ProgressProcessor.GetSongLevel)
			.OrderBy(_LevelIDs => _LevelIDs.Key)
			.ToDictionary(_LevelIDs => _LevelIDs.Key, _LevelIDs => _LevelIDs.ToArray());
	}

	public bool IsSongLockedByLevel(string _SongID)
	{
		if (IsSongAvailable(_SongID))
			return false;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_ProgressProcessor.GetSongLevel(_SongID);
		
		return currentLevel < requiredLevel;
	}

	public bool IsSongLockedByCoins(string _SongID)
	{
		if (IsSongAvailable(_SongID))
			return false;
		
		return m_SongsProcessor.GetPrice(_SongID) > 0;
	}

	public bool IsSongAvailable(string _SongID)
	{
		return m_ProfileProcessor.HasSong(_SongID);
	}
}