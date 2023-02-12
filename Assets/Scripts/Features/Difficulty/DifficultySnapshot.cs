using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class DifficultySnapshot : Snapshot
{
	public RankType Type              { get; }
	public float    Speed             { get; }
	public long     BronzeCoins       { get; }
	public long     SilverCoins       { get; }
	public long     GoldCoins         { get; }
	public long     PlatinumCoins     { get; }
	public int      BronzeThreshold   { get; }
	public int      SilverThreshold   { get; }
	public int      GoldThreshold     { get; }
	public int      PlatinumThreshold { get; }
	public long     BronzePoints      { get; }
	public long     SilverPoints      { get; }
	public long     GoldPoints        { get; }
	public long     PlatinumPoints    { get; }
	public float    InputOffset       { get; }
	public float    InputExpand       { get; }

	public DifficultySnapshot(DataSnapshot _Data) : base(_Data)
	{
		Type              = _Data.GetEnum<RankType>("type");
		Speed             = _Data.GetFloat("speed");
		BronzeCoins       = _Data.GetLong("bronze/coins");
		SilverCoins       = _Data.GetLong("silver/coins");
		GoldCoins         = _Data.GetLong("gold/coins");
		PlatinumCoins     = _Data.GetLong("platinum/coins");
		BronzeThreshold   = _Data.GetInt("bronze/threshold");
		SilverThreshold   = _Data.GetInt("silver/threshold");
		GoldThreshold     = _Data.GetInt("gold/threshold");
		PlatinumThreshold = _Data.GetInt("platinum/threshold");
		BronzePoints      = _Data.GetLong("bronze/points");
		SilverPoints      = _Data.GetLong("silver/points");
		GoldPoints        = _Data.GetLong("gold/points");
		PlatinumPoints    = _Data.GetLong("platinum/points");
		InputOffset       = _Data.GetFloat("input_offset");
		InputExpand       = _Data.GetFloat("input_expand");
	}
}
