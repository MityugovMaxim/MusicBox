using System.Collections.Generic;
using AudioBox.Compression;

public class ChestReward
{
	public RankType Rank      { get; }
	public long     Coins     { get; }
	public string   SongID    { get; }
	public string   VoucherID { get; }

	public ChestReward(Dictionary<string, object> _Data)
	{
		Rank      = _Data.GetEnum<RankType>("rank");
		Coins     = _Data.GetLong("coins");
		SongID    = _Data.GetString("song_id");
		VoucherID = _Data.GetString("voucher_id");
	}
}
