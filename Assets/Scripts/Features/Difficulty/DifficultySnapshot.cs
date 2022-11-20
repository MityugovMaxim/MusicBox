using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class DifficultySnapshot : Snapshot
{
	public DifficultyType Type              { get; }
	public float          Speed             { get; }
	public long           BronzeCoins       { get; }
	public long           SilverCoins       { get; }
	public long           GoldCoins         { get; }
	public long           PlatinumCoins     { get; }
	public int            BronzeThreshold   { get; }
	public int            SilverThreshold   { get; }
	public int            GoldThreshold     { get; }
	public int            PlatinumThreshold { get; }
	public float          InputOffset       { get; }
	public float          InputExpand       { get; }

	public DifficultySnapshot(DataSnapshot _Data) : base(_Data)
	{
		Type              = _Data.GetEnum<DifficultyType>();
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

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["type"]               = (int)Type;
		_Data["speed"]              = Speed;
		_Data["bronze_coins"]       = BronzeCoins;
		_Data["silver_coins"]       = SilverCoins;
		_Data["gold_coins"]         = GoldCoins;
		_Data["platinum_coins"]     = PlatinumCoins;
		_Data["bronze_threshold"]   = BronzeThreshold;
		_Data["silver_threshold"]   = SilverThreshold;
		_Data["gold_threshold"]     = GoldThreshold;
		_Data["platinum_threshold"] = PlatinumThreshold;
		_Data["input_offset"]       = InputOffset;
		_Data["input_expand"]       = InputExpand;
	}
}
