using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public enum ScoreRank
{
	None     = 0,
	Bronze   = 1,
	Silver   = 2,
	Gold     = 3,
	Platinum = 4,
}

[Preserve]
public class ScoreSnapshot : Snapshot
{
	public int       Accuracy { get; }
	public long      Score    { get; }
	public ScoreRank Rank     { get; }
	public int       Rating   { get; }

	public ScoreSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Accuracy = _Data.GetInt("accuracy");
		Score    = _Data.GetLong("score");
		Rank     = _Data.GetEnum<ScoreRank>("rank");
		Rating   = _Data.GetInt("rating");
	}
}

[Preserve]
public class ScoresDataUpdateSignal { }

[Preserve]
public class ScoresProcessor : DataProcessor<ScoreSnapshot, ScoresDataUpdateSignal>
{
	protected override string Path => $"scores/{m_SocialProcessor.UserID}";

	protected override bool SupportsDevelopment => false;

	[Inject] SocialProcessor m_SocialProcessor;

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

	public int GetRating(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Rating ?? 0;
	}
}
