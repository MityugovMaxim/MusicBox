using Firebase.Database;

public class ProfileChest : Snapshot
{
	public RankType   Rank           { get; }
	public ChestState State          { get; }
	public int        Source         { get; }
	public int        Target         { get; }
	public int        Slot           { get; }
	public long       StartTimestamp { get; }
	public long       EndTimestamp   { get; }

	public ProfileChest(DataSnapshot _Data) : base(_Data)
	{
		Rank           = _Data.GetEnum<RankType>("rank");
		State          = _Data.GetEnum<ChestState>("state");
		Source         = _Data.GetInt("source");
		Target         = _Data.GetInt("target");
		Slot           = _Data.GetInt("slot");
		StartTimestamp = _Data.GetLong("start_timestamp");
		EndTimestamp   = _Data.GetLong("end_timestamp");
	}
}
