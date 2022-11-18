using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

[Preserve]
public class DailyCollection : DataCollection<DailySnapshot>
{
	protected override string Path => "daily";

	public List<string> GetDailyIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public long GetCoins(string _DailyID)
	{
		DailySnapshot snapshot = GetSnapshot(_DailyID);
		
		return snapshot?.Coins ?? 0;
	}

	public bool GetAds(string _DailyID)
	{
		DailySnapshot snapshot = GetSnapshot(_DailyID);
		
		return snapshot?.Ads ?? false;
	}
}