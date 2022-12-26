using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsManager : IDataManager
{
	public SongsCollection Collection => m_SongsCollection;

	public ProfileSongs Profile => m_ProfileSongs;

	[Inject] SongsCollection   m_SongsCollection;
	[Inject] ProfileSongs      m_ProfileSongs;
	[Inject] DifficultyManager m_DifficultyManager;
	[Inject] ScoresManager     m_ScoresManager;
	[Inject] MenuProcessor     m_MenuProcessor;

	public Task<bool> Activate()
	{
		return GroupTask.ProcessAsync(
			this,
			Collection.Load,
			Profile.Load
		);
	}

	public List<string> GetAvailableSongIDs()
	{
		return m_SongsCollection.GetIDs()
			.Where(IsFree)
			.OrderBy(m_ScoresManager.GetRank)
			.ThenBy(GetRank)
			.ThenBy(m_SongsCollection.GetOrder)
			.ToList();
	}

	public List<string> GetPaidSongIDs()
	{
		return m_SongsCollection.GetIDs()
			.Where(IsPaid)
			.OrderBy(GetPrice)
			.ThenBy(GetRank)
			.ThenBy(m_SongsCollection.GetOrder)
			.ToList();
	}

	public List<string> GetChestSongIDs()
	{
		return m_SongsCollection.GetIDs()
			.Where(IsChest)
			.OrderBy(GetPrice)
			.ThenBy(GetRank)
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

	public bool IsFree(string _SongID) => Profile.Contains(_SongID) || GetMode(_SongID) == SongMode.Free;

	public bool IsPaid(string _SongID) => !IsFree(_SongID) && GetMode(_SongID) == SongMode.Paid;

	public bool IsChest(string _SongID) => !IsFree(_SongID) && GetMode(_SongID) == SongMode.Chest;

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

	public float GetSpeed(string _SongID) => m_DifficultyManager.GetSpeed(GetRank(_SongID));

	public RankType GetRank(string _SongID)
	{
		SongSnapshot snapshot = m_SongsCollection.GetSnapshot(_SongID);
		
		return snapshot?.Rank ?? RankType.None;
	}

	public long GetCoins(string _SongID, RankType _ScoreRank) => m_DifficultyManager.GetCoins(GetRank(_SongID), _ScoreRank);

	public int GetThreshold(string _SongID, RankType _ScoreRank) => m_DifficultyManager.GetThreshold(GetRank(_SongID), _ScoreRank);

	public RankType GetRank(string _SongID, int _Accuracy) => m_DifficultyManager.GetRank(GetRank(_SongID), _Accuracy);

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

	public async Task<bool> Collect(string _SongID)
	{
		if (string.IsNullOrEmpty(_SongID))
			return false;
		
		if (Profile.Contains(_SongID))
			return false;
		
		if (IsPaid(_SongID) || IsChest(_SongID))
			return false;
		
		Log.Info(this, "Collecting song with ID '{0}'...", _SongID);
		
		SongCollectRequest request = new SongCollectRequest(_SongID);
		
		bool success = await request.SendAsync();
		
		if (success)
			return true;
		
		await m_MenuProcessor.ErrorAsync("song_collect");
		
		return false;
	}
}
