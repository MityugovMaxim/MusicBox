using Firebase.Database;

public class ProfileVoucher : Snapshot
{
	public long Timestamp { get; }

	public ProfileVoucher(DataSnapshot _Data) : base(_Data)
	{
		Timestamp = _Data.GetLong("timestamp");
	}
}
