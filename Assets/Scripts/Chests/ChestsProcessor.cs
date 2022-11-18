using Firebase.Database;

public enum ChestType
{
	None     = 0,
	Bronze   = 1,
	Silver   = 2,
	Gold     = 3,
	Platinum = 4,
}

public class ChestSnapshot : Snapshot
{
	public ChestType Type       { get; }
	public long      Price      { get; }
	public long      Expiration { get; }

	public ChestSnapshot(string _ID, int _Order) : base(_ID, _Order)
	{
		
	}

	protected ChestSnapshot(DataSnapshot       _Data) : base(_Data) { }
}

public class ChestsProcessor
{
	
}
