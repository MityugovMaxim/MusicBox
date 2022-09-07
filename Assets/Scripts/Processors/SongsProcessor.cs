using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

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
	public float     BPM               { get; }
	public int       Bar               { get; }
	public float     Speed             { get; }
	public long      DefaultPayout     { get; }
	public long      BronzePayout      { get; }
	public long      SilverPayout      { get; }
	public long      GoldPayout        { get; }
	public long      PlatinumPayout    { get; }
	public long      Price             { get; }
	public string    Skin              { get; }
	public int       BronzeThreshold   { get; }
	public int       SilverThreshold   { get; }
	public int       GoldThreshold     { get; }
	public int       PlatinumThreshold { get; }
	public string    SpotifyURL        { get; }
	public string    AppleMusicURL     { get; }
	public string    EpidemicSoundURL  { get; }
	public string    DeezerURL         { get; }

	public SongSnapshot() : base("new_song_id", 0)
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
		SpotifyURL        = string.Empty;
		AppleMusicURL     = string.Empty;
		EpidemicSoundURL  = string.Empty;
		DeezerURL         = string.Empty;
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
		SpotifyURL        = _Data.GetString("spotify_url");
		AppleMusicURL     = _Data.GetString("apple_music_url");
		EpidemicSoundURL  = _Data.GetString("epidemic_sound_url");
		DeezerURL         = _Data.GetString("deezer_url");
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
		_Data["spotify_url"]        = SpotifyURL;
		_Data["apple_music_url"]    = AppleMusicURL;
		_Data["epidemic_sound_url"] = EpidemicSoundURL;
		_Data["deezer_url"]         = DeezerURL;
	}
}

[Preserve]
public class SongsDataUpdateSignal { }

[Preserve]
public class SongsProcessor : DataProcessor<SongSnapshot, SongsDataUpdateSignal>
{
	protected override string Path => "songs"; 

	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] MapsProcessor    m_MapsProcessor;

	public List<string> GetSongIDs(bool _IncludeInactive = false)
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _IncludeInactive || _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetSongID(string _SongHash)
	{
		if (string.IsNullOrEmpty(_SongHash))
			return null;
		
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Select(_Snapshot => _Snapshot.ID)
			.FirstOrDefault(_SongID => GetSongHash(_SongID).Equals(_SongHash, StringComparison.InvariantCultureIgnoreCase));
	}

	public string GetSongHash(string _SongID)
	{
		return !string.IsNullOrEmpty(_SongID) ? CRC32.Get(_SongID) : string.Empty;
	}

	public int GetNumber(string _SongID)
	{
		int number = Snapshots
			.Where(_Snapshot => _Snapshot != null && _Snapshot.ID == _SongID)
			.Where(_Snapshot => _Snapshot.Active)
			.OrderBy(_Snapshot => _Snapshot.Speed)
			.Select((_Snapshot, _Index) => _Index)
			.FirstOrDefault();
		
		return number;
	}

	public string GetSkin(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Skin ?? "default";
	}

	public string GetArtist(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			snapshot = m_MapsProcessor.GetSnapshot(_SongID);
		
		return snapshot.Artist ?? string.Empty;
	}

	public string GetTitle(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			snapshot = m_MapsProcessor.GetSnapshot(_SongID);
		
		return snapshot.Title ?? string.Empty;
	}

	public string GetImage(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			snapshot = m_MapsProcessor.GetSnapshot(_SongID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetPreview(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			snapshot = m_MapsProcessor.GetSnapshot(_SongID);
		
		return snapshot?.Preview ?? string.Empty;
	}

	public string GetMusic(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Music ?? string.Empty;
	}

	public long GetPayout(string _SongID, ScoreRank _Rank)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			snapshot = m_MapsProcessor.GetSnapshot(_SongID);
		
		switch (_Rank)
		{
			case ScoreRank.Bronze:   return snapshot.BronzePayout;
			case ScoreRank.Silver:   return snapshot.SilverPayout;
			case ScoreRank.Gold:     return snapshot.GoldPayout;
			case ScoreRank.Platinum: return snapshot.PlatinumPayout;
			default:                 return snapshot.DefaultPayout;
		}
	}

	public long GetPrice(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Price ?? 0;
	}

	public string GetAppleMusicURL(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.AppleMusicURL;
	}

	public string GetSpotifyURL(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.SpotifyURL;
	}

	public string GetDeezerURL(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.DeezerURL;
	}

	public string GetEpidemicSoundURL(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.EpidemicSoundURL;
	}

	public float GetBPM(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.BPM ?? 0;
	}

	public int GetBar(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Bar ?? 0;
	}

	public float GetSpeed(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Speed ?? 0;
	}

	public ScoreRank GetRank(string _SongID, int _Accuracy)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			return ScoreRank.None;
		
		if (_Accuracy >= snapshot.PlatinumThreshold)
			return ScoreRank.Platinum;
		else if (_Accuracy >= snapshot.GoldThreshold)
			return ScoreRank.Gold;
		else if (_Accuracy >= snapshot.SilverThreshold)
			return ScoreRank.Silver;
		else if (_Accuracy >= snapshot.BronzeThreshold)
			return ScoreRank.Bronze;
		else
			return ScoreRank.None;
	}

	public int GetThreshold(string _SongID, ScoreRank _Rank)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			return 0;
		
		switch (_Rank)
		{
			case ScoreRank.Platinum:
				return snapshot.PlatinumThreshold;
			
			case ScoreRank.Gold:
				return snapshot.GoldThreshold;
			
			case ScoreRank.Silver:
				return snapshot.SilverThreshold;
			
			case ScoreRank.Bronze:
				return snapshot.BronzeThreshold;
			
			default:
				return 0;
		}
	}

	public SongMode GetMode(string _SongID)
	{
		if (m_ProfileProcessor.HasNoAds())
			return SongMode.Free;
		
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Mode ?? SongMode.Free;
	}

	public SongBadge GetBadge(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Badge ?? SongBadge.None;
	}
}