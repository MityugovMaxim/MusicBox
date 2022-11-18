using System.Collections.Generic;
using Firebase.Database;

public class SeasonItem
{
	public int          Level      { get; }
	public long         Coins      { get; }
	public List<string> SongIDs    { get; }
	public List<string> VoucherIDs { get; }

	public SeasonItem(DataSnapshot _Data)
	{
		Level      = _Data.GetInt("level");
		Coins      = _Data.GetLong("coins");
		SongIDs    = _Data.GetChildKeys("song_ids");
		VoucherIDs = _Data.GetChildKeys("voucher_ids");
	}
}