using Firebase.Database;

public class ScoreSnapshot : Snapshot
{
	public string    ID       { get; }
	public long      Score    { get; }
	public int       Accuracy { get; }
	public ScoreRank Rank     { get; }

	public ScoreSnapshot(DataSnapshot _Data) : base(_Data)
	{
		ID       = _Data.Key;
		Accuracy = _Data.GetInt("accuracy");
		Score    = _Data.GetLong("score");
		Rank     = _Data.GetEnum<ScoreRank>("rank");
	}
}
