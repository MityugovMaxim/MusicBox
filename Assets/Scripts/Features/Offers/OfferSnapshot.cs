using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class OfferSnapshot : Snapshot
{
	public bool   Active    { get; }
	public string Image     { get; }
	public string SongID    { get; }
	public string VoucherID { get; }
	public string ChestID   { get; }
	public long   Coins     { get; }
	public int    AdsCount  { get; }
	public long   Timestamp { get; }

	public OfferSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/Offers/{ID}.jpg");
		SongID    = _Data.GetString("song_id");
		ChestID   = _Data.GetString("chest_id");
		VoucherID = _Data.GetString("voucher_id");
		Coins     = _Data.GetLong("coins");
		AdsCount  = _Data.GetInt("ads_count");
		Timestamp = _Data.GetLong("timestamp");
	}
}
