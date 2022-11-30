using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsManager
{
	public SongsCollection Collection => m_SongsCollection;

	public ProfileSongs Profile => m_ProfileSongs;

	[Inject] SongsCollection   m_SongsCollection;
	[Inject] ProfileSongs      m_ProfileSongs;
	[Inject] DifficultyManager m_DifficultyManager;
	[Inject] ScoresManager     m_ScoresManager;

	public List<string> GetAvailableSongIDs()
	{
		return m_SongsCollection.GetIDs()
			.Where(IsAvailable)
			.OrderBy(m_ScoresManager.GetRank)
			.ThenBy(GetDifficulty)
			.ThenBy(m_SongsCollection.GetOrder)
			.ToList();
	}

	public List<string> GetPaidSongIDs()
	{
		return m_SongsCollection.GetIDs()
			.Where(IsPaid)
			.OrderBy(GetPrice)
			.ThenBy(GetDifficulty)
			.ThenBy(m_SongsCollection.GetOrder)
			.ToList();
	}

	public List<string> GetChestSongIDs()
	{
		return m_SongsCollection.GetIDs()
			.Where(IsChest)
			.OrderBy(GetPrice)
			.ThenBy(GetDifficulty)
			.ThenBy(m_SongsCollection.GetOrder)
			.ToList();
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

	public bool IsAvailable(string _SongID) => Profile.Contains(_SongID) || GetMode(_SongID) == SongMode.Free;

	public bool IsUnavailable(string _SongID) => !IsAvailable(_SongID);

	public bool IsPaid(string _SongID) => IsUnavailable(_SongID) && GetMode(_SongID) == SongMode.Paid;

	public bool IsChest(string _SongID) => IsUnavailable(_SongID) && GetMode(_SongID) == SongMode.Chest;

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

	// TODO: Remove
	public float GetBPM(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.BPM ?? 0;
	}

	// TODO: Remove
	public int GetBar(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Bar ?? 0;
	}

	// TODO: Remove
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
}
