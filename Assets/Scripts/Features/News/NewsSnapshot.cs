using System.Collections.Generic;
using Firebase.Database;

public class NewsSnapshot : Snapshot
{
	public bool   Active    { get; }
	public string Image     { get; }
	public long   Timestamp { get; }
	public string URL       { get; }

	public NewsSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/News/{ID}.jpg");
		Timestamp = _Data.GetLong("timestamp");
		URL       = _Data.GetString("url");
	}
}
