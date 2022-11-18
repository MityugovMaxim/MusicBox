using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsManager : ProfileCollection<long>
{
	public SongsCollection Collection => m_SongsCollection;

	protected override string Name => "songs";

	[Inject] SongsCollection m_SongsCollection;

	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] ScoresManager     m_ScoresManager;
	[Inject] LevelParameter    m_LevelParameter;

	public List<string> GetLibrarySongIDs()
	{
		IEnumerable<string> availableSongIDs = m_SongsCollection.GetIDs()
			.Where(IsSongAvailable)
			.OrderBy(m_ScoresManager.GetRank)
			.ThenBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(GetSpeed)
			.ThenBy(m_SongsCollection.GetOrder);
		
		IEnumerable<string> adsSongIDs = m_SongsCollection.GetIDs()
			.Where(IsSongLockedByAds)
			.OrderBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(GetSpeed)
			.ThenBy(m_SongsCollection.GetOrder);
		
		IEnumerable<string> paidSongIDs = m_SongsCollection.GetIDs()
			.Where(IsSongLockedByCoins)
			.OrderBy(GetPrice)
			.ThenBy(m_ProgressProcessor.GetSongLevel)
			.ThenBy(GetSpeed)
			.ThenBy(m_SongsCollection.GetOrder);
		
		return availableSongIDs
			.Union(adsSongIDs)
			.Union(paidSongIDs)
			.Distinct()
			.ToList();
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

	public bool IsSongLockedByAds(string _SongID)
	{
		if (IsSongAvailable(_SongID))
			return false;
		
		int currentLevel  = m_LevelParameter.Value;
		int requiredLevel = m_ProgressProcessor.GetSongLevel(_SongID);
		
		return currentLevel >= requiredLevel && GetMode(_SongID) == SongMode.Ads;
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

	public long GetPayout(string _SongID, ScoreRank _Rank)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		if (snapshot == null)
			return 0;
		
		switch (_Rank)
		{
			case ScoreRank.Bronze:   return snapshot.BronzePayout;
			case ScoreRank.Silver:   return snapshot.SilverPayout;
			case ScoreRank.Gold:     return snapshot.GoldPayout;
			case ScoreRank.Platinum: return snapshot.PlatinumPayout;
			default:                 return snapshot.DefaultPayout;
		}
	}

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

	public float GetSpeed(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Speed ?? 0;
	}

	public ScoreRank GetRank(string _SongID, int _Accuracy)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		if (snapshot == null)
			return ScoreRank.None;
		
		if (_Accuracy >= snapshot.PlatinumThreshold)
			return ScoreRank.Platinum;
		
		if (_Accuracy >= snapshot.GoldThreshold)
			return ScoreRank.Gold;
		
		if (_Accuracy >= snapshot.SilverThreshold)
			return ScoreRank.Silver;
		
		if (_Accuracy >= snapshot.BronzeThreshold)
			return ScoreRank.Bronze;
		
		return ScoreRank.None;
	}

	public int GetThreshold(string _SongID, ScoreRank _Rank)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		if (snapshot == null)
			return 0;
		
		switch (_Rank)
		{
			case ScoreRank.Platinum:
				return snapshot.PlatinumThreshold;
			
			case ScoreRank.Gold:
				return snapshot.GoldThreshold;
			
			case ScoreRank.Silver:
				return snapshot.SilverThreshold;
			
			case ScoreRank.Bronze:
				return snapshot.BronzeThreshold;
			
			default:
				return 0;
		}
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
