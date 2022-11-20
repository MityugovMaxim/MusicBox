using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsManager : ProfileCollection<DataSnapshot<long>>
{
	public SongsCollection Collection => m_SongsCollection;

	protected override string Name => "songs";

	[Inject] SongsCollection m_SongsCollection;

	[Inject] DifficultyManager m_DifficultyManager;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] ScoresManager     m_ScoresManager;
	[Inject] LevelParameter    m_LevelParameter;

	public List<string> GetLibrarySongIDs()
	{
		IEnumerable<string> freeSongIDs = m_SongsCollection.GetIDs()
			.Where(IsSongAvailable)
			.OrderBy(m_ScoresManager.GetRank)
			.ThenBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(GetSpeed)
			.ThenBy(m_SongsCollection.GetOrder);
		
		IEnumerable<string> paidSongIDs = m_SongsCollection.GetIDs()
			.Where(IsSongLockedByCoins)
			.OrderBy(GetPrice)
			.ThenBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(GetSpeed)
			.ThenBy(m_SongsCollection.GetOrder);
		
		return freeSongIDs.Union(paidSongIDs).Distinct().ToList();
	}

	public Dictionary<int, List<string>> GetLockedSongIDs()
	{
		return m_SongsCollection.GetIDs()
			.Where(IsSongLockedByLevel)
			.GroupBy(m_ProgressProcessor.GetSongLevel)
			.OrderBy(_LevelIDs => _LevelIDs.Key)
			.ToDictionary(_LevelIDs => _LevelIDs.Key, _LevelIDs => _LevelIDs.ToList());
	}

	public string GetSongID(string _SongHash)
	{
		if (string.IsNullOrEmpty(_SongHash))
			return null;
		
		foreach (string songID in m_SongsCollection.GetIDs())
		{
			if (string.IsNullOrEmpty(songID))
				continue;
			
			string songHash = CRC32.Get(songID);
			
			if (songHash == _SongHash)
				return songID;
		}
		
		return null;
	}

	public bool IsSongLockedByLevel(string _SongID)
	{
		if (IsSongAvailable(_SongID))
			return false;
		
		int currentLevel  = m_LevelParameter.Value;
		int requiredLevel = m_ProgressProcessor.GetSongLevel(_SongID);
		
		return currentLevel < requiredLevel;
	}

	public bool IsSongLockedByCoins(string _SongID)
	{
		if (IsSongAvailable(_SongID))
			return false;
		
		int currentLevel  = m_LevelParameter.Value;
		int requiredLevel = m_ProgressProcessor.GetSongLevel(_SongID);
		
		return currentLevel >= requiredLevel && GetMode(_SongID) == SongMode.Paid;
	}

	public string GetArtist(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Artist ?? string.Empty;
	}

	public string GetTitle(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Title ?? string.Empty;
	}

	public string GetImage(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetPreview(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Preview ?? string.Empty;
	}

	public string GetMusic(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Music ?? string.Empty;
	}

	public string GetASF(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.ASF ?? string.Empty;
	}

	public float GetSpeed(string _SongID) => m_DifficultyManager.GetSpeed(GetDifficulty(_SongID));

	public DifficultyType GetDifficulty(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Difficulty ?? DifficultyType.Casual;
	}

	public long GetCoins(string _SongID, ScoreRank _ScoreRank) => m_DifficultyManager.GetCoins(GetDifficulty(_SongID), _ScoreRank);

	public int GetThreshold(string _SongID, ScoreRank _ScoreRank) => m_DifficultyManager.GetThreshold(GetDifficulty(_SongID), _ScoreRank);

	public ScoreRank GetRank(string _SongID, int _Accuracy) => m_DifficultyManager.GetRank(GetDifficulty(_SongID), _Accuracy);

	public long GetPrice(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Price ?? 0;
	}

	public float GetBPM(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.BPM ?? 0;
	}

	public int GetBar(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Bar ?? 0;
	}

	public double GetOrigin(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Origin ?? 0;
	}

	public SongMode GetMode(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Mode ?? SongMode.Free;
	}

	public SongBadge GetBadge(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Badge ?? SongBadge.None;
	}

	public bool IsSongAvailable(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot != null && snapshot.Active && Contains(_SongID);
	}
}
