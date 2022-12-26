using Firebase.Database;

public class DailySnapshot : Snapshot
{
	public bool Active   { get; }
	public bool Ads      { get; }
	public long Cooldown { get; }
	public long Coins    { get; }

	public DailySnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active   = _Data.GetBool("active");
		Cooldown = _Data.GetLong("cooldown");
		Coins    = _Data.GetLong("coins");
		Ads      = _Data.GetBool("ads");
	}
}
