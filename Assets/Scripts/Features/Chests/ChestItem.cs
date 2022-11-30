using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ChestItem
{
	public ChestItemType Type   { get; }
	public double        Weight { get; }
	public double        Value  { get; }

	public ChestItem(DataSnapshot _Data)
	{
		Type   = _Data.GetEnum<ChestItemType>("type");
		Weight = _Data.GetDouble("weight");
		Value  = _Data.GetDouble("value");
	}
}
