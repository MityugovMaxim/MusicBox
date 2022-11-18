using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class SongSnapshot : Snapshot
{
	public bool      Active            { get; }
	public string    Title             { get; }
	public string    Artist            { get; }
	public string    Image             { get; }
	public string    Preview           { get; }
	public string    Music             { get; }
	public SongMode  Mode              { get; }
	public SongBadge Badge             { get; }

	// TODO: Remove
	public float     BPM               { get; }
	// TODO: Remove
	public int       Bar               { get; }
	// TODO: Remove
	public double    Origin            { get; }

	public float  Speed             { get; }
	public long   DefaultPayout     { get; }
	public long   BronzePayout      { get; }
	public long   SilverPayout      { get; }
	public long   GoldPayout        { get; }
	public long   PlatinumPayout    { get; }
	public long   Price             { get; }
	public string Skin              { get; }
	public int    BronzeThreshold   { get; }
	public int    SilverThreshold   { get; }
	public int    GoldThreshold     { get; }
	public int    PlatinumThreshold { get; }

	public SongSnapshot() : base("song", 0)
	{
		Active            = false;
		Title             = string.Empty;
		Artist            = string.Empty;
		Image             = "Thumbnails/Songs/new_song_id.jpg";
		Preview           = "Previews/new_song_id.ogg";
		Music             = "Songs/new_song_id.ogg";
		Mode              = SongMode.Free;
		Badge             = SongBadge.New;
		BPM               = 120;
		Bar               = 4;
		Origin            = 0;
		Speed             = 850;
		DefaultPayout     = 5;
		BronzePayout      = 10;
		SilverPayout      = 20;
		GoldPayout        = 50;
		PlatinumPayout    = 100;
		Price             = 0;
		Skin              = "default";
		BronzeThreshold   = 5;
		SilverThreshold   = 40;
		GoldThreshold     = 60;
		PlatinumThreshold = 80;
	}

	public SongSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active            = _Data.GetBool("active");
		Title             = _Data.GetString("title", string.Empty);
		Artist            = _Data.GetString("artist", string.Empty);
		Image             = _Data.GetString("image", $"Thumbnails/Songs/{ID}.jpg");
		Preview           = _Data.GetString("preview", $"Previews/{ID}.ogg");
		Music             = _Data.GetString("music", $"Songs/{ID}.ogg");
		Mode              = _Data.GetEnum<SongMode>("mode");
		Badge             = _Data.GetEnum<SongBadge>("badge");
		BPM               = _Data.GetFloat("bpm");
		Bar               = _Data.GetInt("bar", 4);
		Origin            = _Data.GetDouble("origin", 0);
		Speed             = _Data.GetFloat("speed");
		DefaultPayout     = _Data.GetLong("default_payout");
		BronzePayout      = _Data.GetLong("bronze_payout");
		SilverPayout      = _Data.GetLong("silver_payout");
		GoldPayout        = _Data.GetLong("gold_payout");
		PlatinumPayout    = _Data.GetLong("platinum_payout");
		Price             = _Data.GetLong("price");
		Skin              = _Data.GetString("skin", "default");
		BronzeThreshold   = _Data.GetInt("bronze_threshold");
		SilverThreshold   = _Data.GetInt("silver_threshold");
		GoldThreshold     = _Data.GetInt("gold_threshold");
		PlatinumThreshold = _Data.GetInt("platinum_threshold");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]             = Active;
		_Data["title"]              = Title;
		_Data["artist"]             = Artist;
		_Data["image"]              = Image;
		_Data["preview"]            = Preview;
		_Data["music"]              = Music;
		_Data["mode"]               = (int)Mode;
		_Data["badge"]              = (int)Badge;
		_Data["bpm"]                = BPM;
		_Data["bar"]                = Bar;
		_Data["origin"]             = Origin;
		_Data["speed"]              = Speed;
		_Data["default_payout"]     = DefaultPayout;
		_Data["bronze_payout"]      = BronzePayout;
		_Data["silver_payout"]      = SilverPayout;
		_Data["gold_payout"]        = GoldPayout;
		_Data["platinum_payout"]    = PlatinumPayout;
		_Data["price"]              = Price;
		_Data["skin"]               = Skin;
		_Data["bronze_threshold"]   = BronzeThreshold;
		_Data["silver_threshold"]   = SilverThreshold;
		_Data["gold_threshold"]     = GoldThreshold;
		_Data["platinum_threshold"] = PlatinumThreshold;
	}

	public override string ToString()
	{
		return $"{Title} - {Artist}";
	}
}
