using System.Collections.Generic;
using Firebase.Database;

public class ProfileSeason : Snapshot
{
	public long         Points  { get; }
	public List<string> ItemIDs { get; }

	public ProfileSeason(DataSnapshot _Data) : base(_Data)
	{
		Points  = _Data.GetLong("points");
		ItemIDs = _Data.GetChildKeys("item_ids");
	}
}
