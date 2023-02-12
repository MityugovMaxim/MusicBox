using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ChestItem
{
	public RankType SongRank  { get; }
	public string   VoucherID { get; }
	public long     Coins     { get; }
	public long     Points    { get; }
	public double   Weight    { get; }

	public ChestItem(DataSnapshot _Data)
	{
		SongRank  = _Data.GetEnum<RankType>("song_rank");
		VoucherID = _Data.GetString("voucher_id");
		Coins     = _Data.GetLong("coins");
		Points    = _Data.GetLong("points");
		Weight    = _Data.GetDouble("weight");
	}
}
