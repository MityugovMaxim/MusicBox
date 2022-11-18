using System.Collections.Generic;
using Firebase.Database;

public class NewsSnapshot : Snapshot
{
	public bool   Active    { get; }
	public string Image     { get; }
	public long   Timestamp { get; }
	public string URL       { get; }

	public NewsSnapshot() : base("new_news", 0)
	{
		Active    = false;
		Image     = "Thumbnails/News/new_news.jpg";
		Timestamp = TimeUtility.GetTimestamp();
		URL       = "audiobox://";
	}

	public NewsSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/News/{ID}.jpg");
		Timestamp = _Data.GetLong("timestamp");
		URL       = _Data.GetString("url");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]    = Active;
		_Data["image"]     = Image;
		_Data["timestamp"] = Timestamp;
		_Data["url"]       = URL;
	}
}