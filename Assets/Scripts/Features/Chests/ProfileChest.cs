using Firebase.Database;

public class ProfileChest : Snapshot
{
	public ChestType Type  { get; }
	public int       Count { get; }
	public int       Slot  { get; }

	public ProfileChest(DataSnapshot _Data) : base(_Data)
	{
		Type  = _Data.GetEnum<ChestType>("type");
		Count = _Data.GetInt("count");
		Slot  = _Data.GetInt("slot");
	}
}
