using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UIStoreBadge : UIBadge
{
	public const string PRODUCTS_GROUP = "products";

	[Inject] ProductsManager m_ProductsManager;
	[Inject] DailyManager        m_DailyManager;

	List<string> m_DailyIDs;

	protected override void Subscribe()
	{
		m_DailyIDs = m_DailyManager.GetDailyIDs();
		
		m_ProductsManager.Collection.Subscribe(DataEventType.Add, Process);
		m_ProductsManager.Collection.Subscribe(DataEventType.Remove, Process);
		m_ProductsManager.Collection.Subscribe(DataEventType.Change, Process);
		m_ProductsManager.Profile.Subscribe(DataEventType.Add, Process);
		m_ProductsManager.Profile.Subscribe(DataEventType.Remove, Process);
		m_ProductsManager.Profile.Subscribe(DataEventType.Change, Process);
		
		if (m_DailyIDs == null)
			return;
		
		foreach (string dailyID in m_DailyIDs)
		{
			if (string.IsNullOrEmpty(dailyID))
				continue;
			
			m_DailyManager.SubscribeCollect(dailyID, Process);
			m_DailyManager.SubscribeRestore(dailyID, Process);
		}
	}

	protected override void Unsubscribe()
	{
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Remove, Process);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Change, Process);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Add, Process);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Change, Process);
		
		if (m_DailyIDs == null)
			return;
		
		foreach (string dailyID in m_DailyIDs)
		{
			if (string.IsNullOrEmpty(dailyID))
				continue;
			
			m_DailyManager.UnsubscribeCollect(dailyID, Process);
			m_DailyManager.UnsubscribeRestore(dailyID, Process);
		}
	}

	protected override void Process()
	{
		int value = 0;
		
		value += GetProductsCount();
		value += GetDailyCount();
		
		SetValue(value);
	}

	int GetProductsCount()
	{
		List<string> productIDs = m_ProductsManager.GetProductIDs();
		
		return productIDs?.Count(_ProductID => BadgeManager.IsUnread(PRODUCTS_GROUP, _ProductID)) ?? 0;
	}

	int GetDailyCount()
	{
		List<string> dailyIDs = m_DailyManager.GetDailyIDs();
		
		return dailyIDs?.Count(_DailyID => m_DailyManager.IsDailyAvailable(_DailyID)) ?? 0;
	}
}
