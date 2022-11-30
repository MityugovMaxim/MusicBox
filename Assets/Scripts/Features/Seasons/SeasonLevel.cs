using Firebase.Database;

public class SeasonLevel : Snapshot
{
	public int        Level    { get; }
	public long       Points   { get; }
	public SeasonItem FreeItem { get; }
	public SeasonItem PaidItem { get; }

	public SeasonLevel(DataSnapshot _Data) : base(_Data)
	{
		Level  = _Data.GetInt("level");
		Points = _Data.GetLong("points");
		if (_Data.HasChild("free_item"))
			FreeItem = new SeasonItem(_Data.Child("free_item"));
		if (_Data.HasChild("paid_item"))
			PaidItem = new SeasonItem(_Data.Child("paid_item"));
	}
}
