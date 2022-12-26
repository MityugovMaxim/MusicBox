using Firebase.Database;

public class AmbientSnapshot : Snapshot
{
	public bool   Active { get; }
	public string Title  { get; }
	public string Artist { get; }
	public string Sound  { get; }
	public float  Volume { get; }

	public AmbientSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active = _Data.GetBool("active");
		Title  = _Data.GetString("title");
		Artist = _Data.GetString("artist");
		Sound  = _Data.GetString("sound", $"Ambient/{ID}.ogg");
		Volume = _Data.GetFloat("volume");
	}
}
