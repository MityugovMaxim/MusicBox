using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ScoresManager
{
	public ProfileScores Profile => m_ProfileScores;

	[Inject] ProfileScores m_ProfileScores;
	[Inject] SongsManager  m_SongsManager;

	public string GetBestSongID()
	{
		return Profile.GetIDs()
			.OrderByDescending(GetAccuracy)
			.ThenByDescending(m_SongsManager.GetRank)
			.FirstOrDefault();
	}

	public string GetWorstSongID()
	{
		return Profile.GetIDs()
			.OrderBy(GetAccuracy)
			.ThenBy(m_SongsManager.GetRank)
			.FirstOrDefault();
	}

	public int GetAccuracy(string _SongID)
	{
		ProfileScore snapshot = Profile.GetSnapshot(_SongID);
		
		return snapshot?.Accuracy ?? 0;
	}

	public long GetScore(string _SongID)
	{
		ProfileScore snapshot = Profile.GetSnapshot(_SongID);
		
		return snapshot?.Score ?? 0;
	}

	public RankType GetRank(string _SongID)
	{
		ProfileScore snapshot = Profile.GetSnapshot(_SongID);
		
		return snapshot?.Rank ?? RankType.None;
	}
}
