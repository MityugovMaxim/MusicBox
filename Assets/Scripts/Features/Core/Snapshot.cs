using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class Snapshot
{
	public string ID    { get; }
	public int    Order { get; set; }

	protected Snapshot(string _ID, int _Order)
	{
		ID    = _ID;
		Order = _Order;
	}

	protected Snapshot(DataSnapshot _Data)
	{
		ID    = _Data.Key;
		Order = _Data.GetInt("order");
	}

	public virtual void Serialize(Dictionary<string, object> _Data)
	{
		_Data["order"] = Order;
	}

	public override string ToString() => ID;
}