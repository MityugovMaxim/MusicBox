using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ProfileSeason : Snapshot
{
	public long      Points    { get; }
	public List<int> FreeItems { get; }
	public List<int> PaidItems { get; }

	public ProfileSeason(DataSnapshot _Data) : base(_Data)
	{
		Points    = _Data.GetLong("points");
		FreeItems = _Data.GetIntList("free_items");
		PaidItems = _Data.GetIntList("paid_items");
	}
}
