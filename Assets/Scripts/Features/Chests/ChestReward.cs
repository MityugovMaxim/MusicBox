using System.Collections.Generic;
using AudioBox.Compression;
using UnityEngine.Purchasing.MiniJSON;

public class ChestReward
{
	public string        ID    { get; }
	public ChestItemType Type  { get; }
	public double        Value { get; }

	public bool IsCoins => Type == ChestItemType.Coins;

	public bool IsSong => Type == ChestItemType.SongBronze || Type == ChestItemType.SongSilver || Type == ChestItemType.SongGold || Type == ChestItemType.SongPlatinum;

	public bool IsVoucher => Type == ChestItemType.VoucherCoins || Type == ChestItemType.VoucherStore || Type == ChestItemType.VoucherSongs || Type == ChestItemType.VoucherSeasons;

	public ChestReward(Dictionary<string, object> _Data)
	{
		ID    = _Data.GetString("id");
		Type  = _Data.GetEnum<ChestItemType>("type");
		Value = _Data.GetDouble("value");
	}
}
