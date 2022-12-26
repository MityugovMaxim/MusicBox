using Firebase.Database;

public class ProfileProduct : Snapshot
{
	public long Timestamp { get; }

	public ProfileProduct(DataSnapshot _Data) : base(_Data)
	{
		Timestamp = _Data.GetLong("timestamp");
	}
}
