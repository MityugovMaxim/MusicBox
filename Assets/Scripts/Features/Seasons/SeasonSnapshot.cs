using System.Collections.Generic;
using System.Linq;
using Firebase.Database;

public class SeasonSnapshot : Snapshot
{
	public long              StartTimestamp { get; }
	public long              EndTimestamp   { get; }
	public List<SeasonLevel> Levels         { get; }

	public SeasonSnapshot(DataSnapshot _Data) : base(_Data)
	{
		StartTimestamp = _Data.GetLong("start_timestamp");
		EndTimestamp   = _Data.GetLong("end_timestamp");
		Levels = _Data.Child("levels").Children
			.Select(_Item => new SeasonLevel(_Item))
			.ToList();
	}
}
