using Firebase.Database;

public class Descriptor
{
	public string ID          { get; }
	public string Title       { get; }
	public string Description { get; }

	public Descriptor(DataSnapshot _Data)
	{
		ID          = _Data.Key;
		Title       = _Data.GetString("title");
		Description = _Data.GetString("description");
	}
}