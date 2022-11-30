using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ChestSnapshot : Snapshot
{
	public ChestType       Type      { get; }
	public long            OpenCoins { get; }
	public long            OpenTime  { get; }
	public int             Capacity  { get; }
	public List<ChestItem> Items     { get; }

	public ChestSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Type      = _Data.GetEnum<ChestType>("type");
		OpenCoins = _Data.GetLong("open_coins");
		OpenTime  = _Data.GetLong("open_time");
		Capacity  = _Data.GetInt("capacity");
		Items     = _Data.Child("items").Children.Select(_Item => new ChestItem(_Item)).ToList();
	}
}
