using Firebase.Database;

public class SeasonItem : Snapshot
{
	public long   Coins     { get; }
	public string SongID    { get; }
	public string VoucherID { get; }
	public string ChestID   { get; }
	public string FrameID   { get; }

	public SeasonItem(DataSnapshot _Data) : base(_Data)
	{
		Coins     = _Data.GetLong("coins");
		SongID    = _Data.GetString("song_id");
		VoucherID = _Data.GetString("voucher_id");
		ChestID   = _Data.GetString("chest_id");
		FrameID   = _Data.GetString("frame_id");
	}
}
