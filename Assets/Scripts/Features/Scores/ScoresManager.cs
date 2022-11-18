using UnityEngine.Scripting;

[Preserve]
public class ScoresManager : ProfileCollection<ScoreSnapshot>
{
	protected override string Name => "scores";

	public int GetAccuracy(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Accuracy ?? 0;
	}

	public long GetScore(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Score ?? 0;
	}

	public ScoreRank GetRank(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Rank ?? ScoreRank.None;
	}
}
