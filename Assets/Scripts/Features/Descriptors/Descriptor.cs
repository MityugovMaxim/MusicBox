using Firebase.Database;

public class Descriptor : Snapshot
{
	public string Title       { get; }
	public string Description { get; }

	public Descriptor(DataSnapshot _Data) : base(_Data)
	{
		Title       = _Data.GetString("title");
		Description = _Data.GetString("description");
	}
}
