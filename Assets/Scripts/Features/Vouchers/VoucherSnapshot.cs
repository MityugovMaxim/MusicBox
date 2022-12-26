using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class VoucherSnapshot : Snapshot
{
	public VoucherType           Type           { get; }
	public VoucherGroup          Group          { get; }
	public double                Amount         { get; }
	public long                  StartTimestamp { get; }
	public long                  EndTimestamp   { get; }
	public IReadOnlyList<string> IDs            { get; }

	public VoucherSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Type           = _Data.GetEnum<VoucherType>("type");
		Group          = _Data.GetEnum<VoucherGroup>("group");
		Amount         = _Data.GetDouble("amount");
		StartTimestamp = _Data.GetLong("start_timestamp");
		EndTimestamp   = _Data.GetLong("end_timestamp");
		IDs            = _Data.GetChildKeys("ids");
	}
}
