using System.Collections.Generic;
using System.Threading.Tasks;
using Zenject;

public class SeasonManager : ProfileCollection<DataSnapshot<long>>
{
	protected override string Name => "season";

	[Inject] SeasonsCollection m_SeasonsCollection;

	public bool IsFreeItemCollected(string _BattlePassID, int _Level)
	{
		string itemID = GetFreeItemID(_BattlePassID, _Level);
		
		return Contains(itemID);
	}

	public bool IsFreeItemAvailable(string _BattlePassID, int _Level)
	{
		if (IsUnavailable(_BattlePassID))
			return false;
		
		string itemID = GetFreeItemID(_BattlePassID, _Level);
		
		return !Contains(itemID);
	}

	public Task<bool> CollectFreeItem(string _BattlePassID, int _Level)
	{
		BattlePassCollectRequest request = new BattlePassCollectRequest(_BattlePassID, _Level, true);
		
		return request.SendAsync();
	}

	public Task<bool> CollectPaidItem(string _BattlePassID, int _Level)
	{
		BattlePassCollectRequest request = new BattlePassCollectRequest(_BattlePassID, _Level, false);
		
		return request.SendAsync();
	}

	bool IsAvailable(string _BattlePassID)
	{
		SeasonSnapshot snapshot = m_SeasonsCollection.GetSnapshot(_BattlePassID);
		
		long timestamp = TimeUtility.GetTimestamp();
		
		return snapshot != null && snapshot.StartTimestamp <= timestamp && snapshot.EndTimestamp >= timestamp;
	}

	bool IsUnavailable(string _BattlePassID)
	{
		SeasonSnapshot snapshot = m_SeasonsCollection.GetSnapshot(_BattlePassID);
		
		long timestamp = TimeUtility.GetTimestamp();
		
		return snapshot == null || snapshot.StartTimestamp > timestamp || snapshot.EndTimestamp < timestamp;
	}

	string GetFreeItemID(string _BattlePassID, int _Level) => $"{_BattlePassID}_{_Level}_free";

	string GetPaidItemID(string _BattlePassID, int _Level) => $"{_BattlePassID}_{_Level}_paid";
}
