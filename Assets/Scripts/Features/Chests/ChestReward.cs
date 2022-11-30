using System.Collections.Generic;
using AudioBox.Compression;
using UnityEngine.Purchasing.MiniJSON;

public class ChestReward
{
	public string        ID    { get; }
	public ChestItemType Type  { get; }
	public double        Value { get; }

	public ChestReward(Dictionary<string, object> _Data)
	{
		ID    = _Data.GetString("id");
		Type  = _Data.GetEnum<ChestItemType>("type");
		Value = _Data.GetDouble("value");
	}
}
