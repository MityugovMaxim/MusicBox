using System.Collections.Generic;
using Firebase.Database;

public class DailySnapshot : Snapshot
{
	public bool Active   { get; }
	public long Cooldown { get; }
	public long Coins    { get; }
	public bool Ads      { get; }

	public DailySnapshot() : base("new_daily", 0)
	{
		Active   = false;
		Cooldown = 60000;
		Coins    = 0;
		Ads      = false;
	}

	public DailySnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active   = _Data.GetBool("active");
		Cooldown = _Data.GetLong("cooldown");
		Coins    = _Data.GetLong("coins");
		Ads      = _Data.GetBool("ads");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]   = Active;
		_Data["cooldown"] = Cooldown;
		_Data["coins"]    = Coins;
		_Data["ads"]      = Ads;
	}
}