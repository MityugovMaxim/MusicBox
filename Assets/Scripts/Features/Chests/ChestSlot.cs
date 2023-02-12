using Firebase.Database;

public class ChestSlot : Snapshot
{
	public RankType Rank           { get; }
	public int      Slot           { get; }
	public long     StartTimestamp { get; }
	public long     EndTimestamp   { get; }

	public ChestSlot(DataSnapshot _Data) : base(_Data)
	{
		Rank           = _Data.GetEnum<RankType>("rank");
		Slot           = _Data.GetInt("slot");
		StartTimestamp = _Data.GetLong("start_timestamp");
		EndTimestamp   = _Data.GetLong("end_timestamp");
	}
}
