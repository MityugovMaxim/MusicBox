using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class VoucherSnapshot : Snapshot
{
	public VoucherType           Type       { get; }
	public VoucherGroup          Group      { get; }
	public double                Amount     { get; }
	public long                  Expiration { get; }
	public IReadOnlyList<string> IDs        { get; }

	public VoucherSnapshot(string _ID, int _Order) : base(_ID, _Order)
	{
		Type       = VoucherType.ProductDiscount;
		Group      = VoucherGroup.All;
		Amount     = 0;
		Expiration = 0;
		IDs        = new List<string>();
	}

	public VoucherSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Type       = _Data.GetEnum<VoucherType>("type");
		Group      = _Data.GetEnum<VoucherGroup>("group");
		Amount     = _Data.GetDouble("amount");
		Expiration = _Data.GetLong("expiration");
		IDs        = _Data.GetChildKeys("ids");
	}
}