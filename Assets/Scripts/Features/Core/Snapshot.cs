using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class Snapshot
{
	public string ID { get; }

	public int Order { get; }

	protected Snapshot(DataSnapshot _Data)
	{
		ID    = _Data.Key;
		Order = _Data.GetInt("order");
	}

	public override string ToString() => ID;
}
