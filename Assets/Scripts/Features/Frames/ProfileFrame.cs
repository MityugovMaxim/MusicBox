using Firebase.Database;

public class ProfileFrame : Snapshot
{
	public long Timestamp { get; }

	public ProfileFrame(DataSnapshot _Data) : base(_Data)
	{
		Timestamp = _Data.GetLong();
	}
}