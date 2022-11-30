using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ScoresManager
{
	public ProfileScores Profile => m_ProfileScores;

	[Inject] ProfileScores m_ProfileScores;

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

	public ScoreRank GetRank(string _SongID)
	{
		ProfileScore snapshot = Profile.GetSnapshot(_SongID);
		
		return snapshot?.Rank ?? ScoreRank.None;
	}
}
