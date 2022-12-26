using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ChestSnapshot : Snapshot
{
	public RankType        Rank     { get; }
	public long            Boost    { get; }
	public long            Time     { get; }
	public int             Capacity { get; }
	public List<ChestItem> Items    { get; }

	public ChestSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Rank     = _Data.GetEnum<RankType>("rank");
		Boost    = _Data.GetLong("boost");
		Time     = _Data.GetLong("time");
		Capacity = _Data.GetInt("capacity");
		Items    = _Data.Child("items").Children.Select(_Item => new ChestItem(_Item)).ToList();
	}
}
