using Zenject;

public class UISeasonsBadge : UIBadge
{
	[Inject] SeasonsManager m_SeasonsManager;

	string m_SeasonID;

	protected override void Subscribe()
	{
		m_SeasonID = m_SeasonsManager.GetSeasonID();
		
		if (string.IsNullOrEmpty(m_SeasonID))
			return;
		
		m_SeasonsManager.Profile.Subscribe(DataEventType.Add, m_SeasonID, Process);
		m_SeasonsManager.Profile.Subscribe(DataEventType.Remove, m_SeasonID, Process);
		m_SeasonsManager.Profile.Subscribe(DataEventType.Change, m_SeasonID, Process);
		m_SeasonsManager.Collection.Subscribe(DataEventType.Add, m_SeasonID, Process);
		m_SeasonsManager.Collection.Subscribe(DataEventType.Remove, m_SeasonID, Process);
		m_SeasonsManager.Collection.Subscribe(DataEventType.Change, m_SeasonID, Process);
	}

	protected override void Unsubscribe()
	{
		if (string.IsNullOrEmpty(m_SeasonID))
			return;
		
		m_SeasonsManager.Profile.Unsubscribe(DataEventType.Add, m_SeasonID, Process);
		m_SeasonsManager.Profile.Unsubscribe(DataEventType.Remove, m_SeasonID, Process);
		m_SeasonsManager.Profile.Unsubscribe(DataEventType.Change, m_SeasonID, Process);
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Add, m_SeasonID, Process);
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Remove, m_SeasonID, Process);
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Change, m_SeasonID, Process);
	}

	protected override void Process()
	{
		int value = 0;
		
		value += GetFreeItemsCount();
		value += GetPaidItemsCount();
		
		SetValue(value);
	}

	int GetFreeItemsCount()
	{
		if (string.IsNullOrEmpty(m_SeasonID))
			return 0;
		
		int count = 0;
		
		int source = m_SeasonsManager.GetMinLevel(m_SeasonID);
		int target = m_SeasonsManager.GetLevel(m_SeasonID);
		
		for (int level = source; level <= target; level++)
		{
			string itemID = m_SeasonsManager.GetFreeItemID(m_SeasonID, level);
			
			if (m_SeasonsManager.IsItemAvailable(m_SeasonID, itemID))
				count++;
		}
		
		return count;
	}

	int GetPaidItemsCount()
	{
		if (string.IsNullOrEmpty(m_SeasonID))
			return 0;
		
		int count = 0;
		
		int source = m_SeasonsManager.GetMinLevel(m_SeasonID);
		int target = m_SeasonsManager.GetLevel(m_SeasonID);
		
		for (int level = source; level <= target; level++)
		{
			string itemID = m_SeasonsManager.GetPaidItemID(m_SeasonID, level);
			
			if (m_SeasonsManager.IsItemAvailable(m_SeasonID, itemID))
				count++;
		}
		
		return count;
	}
}
