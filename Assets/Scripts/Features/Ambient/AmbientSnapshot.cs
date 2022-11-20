using System.Collections.Generic;
using Firebase.Database;

public class AmbientSnapshot : Snapshot
{
	public bool   Active { get; }
	public string Title  { get; }
	public string Artist { get; }
	public string Sound  { get; }
	public float  Volume { get; }

	public AmbientSnapshot() : base("AMBIENT", 0)
	{
		Active = false;
		Title  = "TITLE";
		Artist = "ARTIST";
		Sound  = "Ambient/AMBIENT.ogg";
		Volume = 0.5f;
	}

	public AmbientSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active = _Data.GetBool("active");
		Title  = _Data.GetString("title");
		Artist = _Data.GetString("artist");
		Sound  = _Data.GetString("sound", $"Ambient/{ID}.ogg");
		Volume = _Data.GetFloat("volume");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"] = Active;
		_Data["title"]  = Title;
		_Data["artist"] = Artist;
		_Data["sound"]  = Sound;
		_Data["volume"] = Volume;
	}
}
