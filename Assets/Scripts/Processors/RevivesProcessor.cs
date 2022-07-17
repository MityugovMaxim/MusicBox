using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ReviveSnapshot : Snapshot
{
	public bool   Active { get; }
	public int    Count  { get; }
	public long   Coins  { get; }

	public ReviveSnapshot() : base("new_revive", 0)
	{
		Active = false;
		Count  = 0;
		Coins  = 0;
	}

	public ReviveSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active = _Data.GetBool("active");
		Count  = _Data.GetInt("count");
		Coins  = _Data.GetLong("coins");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"] = Active;
		_Data["count"]  = Count;
		_Data["coins"]  = Coins;
	}
}

[Preserve]
public class RevivesDataUpdateSignal { }

[Preserve]
public class RevivesProcessor : DataProcessor<ReviveSnapshot, RevivesDataUpdateSignal>
{
	protected override string Path => "revives";

	public long GetCoins(int _Count)
	{
		if (Snapshots.Count == 0)
			return 0;
		
		int minCount = Snapshots.Min(_Snapshot => _Snapshot.Count);
		int maxCount = Snapshots.Max(_Snapshot => _Snapshot.Count);
		int count    = Mathf.Clamp(_Count, minCount, maxCount);
		
		ReviveSnapshot snapshot = Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Count >= count)
			.Aggregate((_A, _B) => _A.Coins <= _B.Coins ? _A : _B);
		
		return snapshot?.Coins ?? 0;
	}
}