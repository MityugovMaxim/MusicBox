using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ProfileScore : Snapshot
{
	public RankType Rank      { get; }
	public int       Accuracy  { get; }
	public long      Score     { get; }
	public long      Timestamp { get; }

	public ProfileScore(DataSnapshot _Data) : base(_Data)
	{
		Rank      = _Data.GetEnum<RankType>("rank");
		Accuracy  = _Data.GetInt("accuracy");
		Score     = _Data.GetLong("score");
		Timestamp = _Data.GetLong("timestamp");
	}
}
