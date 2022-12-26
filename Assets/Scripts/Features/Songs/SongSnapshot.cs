using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class SongSnapshot : Snapshot
{
	public bool      Active  { get; }
	public string    Title   { get; }
	public string    Artist  { get; }
	public string    Image   { get; }
	public string    Preview { get; }
	public string    Music   { get; }
	public string    ASF     { get; }
	public RankType  Rank    { get; }
	public SongMode  Mode    { get; }
	public SongBadge Badge   { get; }

	// TODO: Remove
	public float     BPM               { get; }
	// TODO: Remove
	public int       Bar               { get; }
	// TODO: Remove
	public double Origin { get; }
	public long   Price  { get; }
	public string Skin   { get; }

	public SongSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active  = _Data.GetBool("active");
		Rank    = _Data.GetEnum<RankType>("rank", RankType.Bronze);
		Title   = _Data.GetString("title", string.Empty);
		Artist  = _Data.GetString("artist", string.Empty);
		Image   = _Data.GetString("image", $"Thumbnails/Songs/{ID}.jpg");
		Preview = _Data.GetString("preview", $"Previews/{ID}.ogg");
		Music   = _Data.GetString("music", $"Songs/{ID}.ogg");
		ASF     = _Data.GetString("asf", $"Songs/{ID}.asf");
		Mode    = _Data.GetEnum<SongMode>("mode");
		Badge   = _Data.GetEnum<SongBadge>("badge");
		BPM     = _Data.GetFloat("bpm");
		Bar     = _Data.GetInt("bar", 4);
		Origin  = _Data.GetDouble("origin", 0);
		Price   = _Data.GetLong("price");
		Skin    = _Data.GetString("skin", "default");
	}

	public override string ToString()
	{
		return $"{Title} - {Artist}";
	}
}
