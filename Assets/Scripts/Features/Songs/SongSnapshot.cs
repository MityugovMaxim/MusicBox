using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class SongSnapshot : Snapshot
{
	public bool     Active  { get; }
	public string   Title   { get; }
	public string   Artist  { get; }
	public string   Image   { get; }
	public string   Preview { get; }
	public string   Music   { get; }
	public string   ASF     { get; }
	public RankType Rank    { get; }
	public SongMode Mode    { get; }
	public long     Price   { get; }
	public string   Skin    { get; }

	public SongSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active  = _Data.GetBool("active");
		Rank    = _Data.GetEnum("rank", RankType.Bronze);
		Title   = _Data.GetString("title", string.Empty);
		Artist  = _Data.GetString("artist", string.Empty);
		Image   = _Data.GetString("image", $"Thumbnails/Songs/{ID}.jpg");
		Preview = _Data.GetString("preview", $"Previews/{ID}.ogg");
		Music   = _Data.GetString("music", $"Songs/{ID}.ogg");
		ASF     = _Data.GetString("asf", $"ASF/{ID}.asf");
		Mode    = _Data.GetEnum("mode", SongMode.Free);
		Price   = _Data.GetLong("price");
		Skin    = _Data.GetString("skin", "default");
	}

	public override string ToString()
	{
		return $"{Title} - {Artist}";
	}
}
