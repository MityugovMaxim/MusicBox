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
	public float    InputOffset       { get; }
	public float    InputExpand       { get; }

	public DifficultySnapshot(DataSnapshot _Data) : base(_Data)
	{
		Type              = _Data.GetEnum<RankType>("rank");
		Speed             = _Data.GetFloat("speed");
		BronzeCoins       = _Data.GetLong("bronze_coins");
		SilverCoins       = _Data.GetLong("silver_coins");
		GoldCoins         = _Data.GetLong("gold_coins");
		PlatinumCoins     = _Data.GetLong("platinum_coins");
		BronzeThreshold   = _Data.GetInt("bronze_threshold");
		SilverThreshold   = _Data.GetInt("silver_threshold");
		GoldThreshold     = _Data.GetInt("gold_threshold");
		PlatinumThreshold = _Data.GetInt("platinum_threshold");
		InputOffset       = _Data.GetFloat("input_offset");
		InputExpand       = _Data.GetFloat("input_expand");
	}
}
