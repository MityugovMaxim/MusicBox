using System.Collections.Generic;
using System.Linq;
using Firebase.Database;

public class SeasonSnapshot : Snapshot
{
	public long             StartTimestamp { get; }
	public long             EndTimestamp   { get; }
	public List<SeasonItem> FreeItems      { get; }
	public List<SeasonItem> PaidItems      { get; }

	protected SeasonSnapshot(string _ID, int _Order) : base(_ID, _Order)
	{
		StartTimestamp = 0;
		EndTimestamp   = 0;
		FreeItems      = new List<SeasonItem>();
		PaidItems      = new List<SeasonItem>();
	}

	protected SeasonSnapshot(DataSnapshot _Data) : base(_Data)
	{
		StartTimestamp = _Data.GetLong("start_timestamp");
		EndTimestamp   = _Data.GetLong("end_timestamp");
		
		FreeItems = _Data.Child("free_items").Children
			.Select(_Item => new SeasonItem(_Item))
			.ToList();
		
		PaidItems = _Data.Child("paid_items").Children
			.Select(_Item => new SeasonItem(_Item))
			.ToList();
	}
}