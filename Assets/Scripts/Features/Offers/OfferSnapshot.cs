using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class OfferSnapshot : Snapshot
{
	public bool   Active    { get; }
	public string Image     { get; }
	public string SongID    { get; }
	public long   Coins     { get; }
	public int    AdsCount  { get; }
	public long   Timestamp { get; }

	public OfferSnapshot() : base("new_offer", 0)
	{
		Active    = false;
		Image     = "Thumbnails/Offers/new_offer.jpg";
		SongID    = string.Empty;
		Coins     = 0;
		AdsCount  = 0;
		Timestamp = 0;
	}

	public OfferSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/Offers/{ID}.jpg");
		SongID    = _Data.GetString("song_id");
		Coins     = _Data.GetLong("coins");
		AdsCount  = _Data.GetInt("ads_count");
		Timestamp = _Data.GetLong("timestamp");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]    = Active;
		_Data["image"]     = Image;
		_Data["song_id"]   = SongID;
		_Data["coins"]     = Coins;
		_Data["ads_count"] = AdsCount;
		_Data["timestamp"] = Timestamp;
	}
}