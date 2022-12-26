using System.Collections.Generic;
using Zenject;

public class UISeasonsBadge : UIBadge
{
	[Inject] SeasonsManager m_SeasonsManager;

	protected override void Subscribe()
	{
		m_SeasonsManager.Profile.Subscribe(DataEventType.Add, Process);
		m_SeasonsManager.Profile.Subscribe(DataEventType.Remove, Process);
		m_SeasonsManager.Profile.Subscribe(DataEventType.Change, Process);
		m_SeasonsManager.Collection.Subscribe(DataEventType.Add, Process);
		m_SeasonsManager.Collection.Subscribe(DataEventType.Remove, Process);
		m_SeasonsManager.Collection.Subscribe(DataEventType.Change, Process);
	}

	protected override void Unsubscribe()
	{
		m_SeasonsManager.Profile.Unsubscribe(DataEventType.Add, Process);
		m_SeasonsManager.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_SeasonsManager.Profile.Unsubscribe(DataEventType.Change, Process);
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Remove, Process);
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Change, Process);
	}

	protected override async void Preload()
	{
		await m_SeasonsManager.Activate();
		
		base.Preload();
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
		string seasonID = m_SeasonsManager.GetSeasonID();
		
		if (string.IsNullOrEmpty(seasonID))
			return 0;
		
		List<int> levels = m_SeasonsManager.GetLevels(seasonID);
		
		if (levels == null)
			return 0;
		
		int count = 0;
		
		foreach (int level in levels)
		{
			if (m_SeasonsManager.IsItemAvailable(seasonID, level, SeasonItemMode.Free))
				count++;
		}
		
		return count;
	}

	int GetPaidItemsCount()
	{
		string seasonID = m_SeasonsManager.GetSeasonID();
		
		if (string.IsNullOrEmpty(seasonID))
			return 0;
		
		List<int> levels = m_SeasonsManager.GetLevels(seasonID);
		
		if (levels == null)
			return 0;
		
		int count = 0;
		
		foreach (int level in levels)
		{
			if (m_SeasonsManager.IsItemAvailable(seasonID, level, SeasonItemMode.Paid))
				count++;
		}
		
		return count;
	}
}
