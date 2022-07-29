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
		List<string> songIDs = m_SongsProcessor.GetSongIDs();
		
		IEnumerable<string> availableSongIDs = songIDs
			.Where(IsSongAvailable)
			.OrderBy(m_ScoresProcessor.GetRank)
			.ThenBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(m_SongsProcessor.GetSpeed)
			.ThenBy(m_SongsProcessor.GetOrder);
		
		IEnumerable<string> adsSongIDs = songIDs
			.Where(IsSongLockedByAds)
			.OrderBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(m_SongsProcessor.GetSpeed)
			.ThenBy(m_SongsProcessor.GetOrder);
		
		IEnumerable<string> paidSongIDs = songIDs
			.Where(IsSongLockedByCoins)
			.OrderBy(m_SongsProcessor.GetPrice)
			.ThenBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(m_SongsProcessor.GetSpeed)
			.ThenBy(m_SongsProcessor.GetOrder);
		
		return availableSongIDs
			.Union(adsSongIDs)
			.Union(paidSongIDs)
			.Distinct()
			.ToList();
	}

	public Dictionary<int, List<string>> GetLockedSongIDs()
	{
		return m_SongsProcessor.GetSongIDs()
			.Where(IsSongLockedByLevel)
			.GroupBy(m_ProgressProcessor.GetSongLevel)
			.OrderBy(_LevelIDs => _LevelIDs.Key)
			.ToDictionary(_LevelIDs => _LevelIDs.Key, _LevelIDs => _LevelIDs.ToList());
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
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_ProgressProcessor.GetSongLevel(_SongID);
		
		return currentLevel >= requiredLevel && m_SongsProcessor.GetMode(_SongID) == SongMode.Paid;
	}

	public bool IsSongLockedByAds(string _SongID)
	{
		if (IsSongAvailable(_SongID))
			return false;
		
		int currentLevel  = m_ProfileProcessor.Level;
		int requiredLevel = m_ProgressProcessor.GetSongLevel(_SongID);
		
		return currentLevel >= requiredLevel && m_SongsProcessor.GetMode(_SongID) == SongMode.Ads;
	}

	public bool IsSongAvailable(string _SongID)
	{
		return m_ProfileProcessor.HasSong(_SongID);
	}
}