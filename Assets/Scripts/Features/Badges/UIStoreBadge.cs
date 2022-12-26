using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

public class UIStoreBadge : UIBadge
{
	[Inject] ProductsManager m_ProductsManager;
	[Inject] DailyManager    m_DailyManager;

	List<string> m_DailyIDs;

	protected override void Subscribe()
	{
		BadgeManager.SubscribeProducts(Process);
		m_ProductsManager.Collection.Subscribe(DataEventType.Add, Reload);
		m_ProductsManager.Collection.Subscribe(DataEventType.Remove, Reload);
		m_ProductsManager.Profile.Subscribe(DataEventType.Add, Reload);
		m_ProductsManager.Profile.Subscribe(DataEventType.Remove, Reload);
	}

	protected override void Unsubscribe()
	{
		BadgeManager.UnsubscribeProducts(Process);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Add, Reload);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Remove, Reload);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Add, Reload);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Remove, Reload);
	}

	protected override async void Preload()
	{
		await Task.WhenAll(
			m_ProductsManager.Activate(),
			m_DailyManager.Activate()
		);
		
		base.Preload();
	}

	void Reload()
	{
		if (m_DailyIDs != null)
		{
			foreach (string dailyID in m_DailyIDs)
			{
				m_DailyManager.UnsubscribeCollect(dailyID, Process);
				m_DailyManager.UnsubscribeRestore(dailyID, Process);
			}
		}
		
		m_DailyIDs = m_DailyManager.GetDailyIDs();
		
		if (m_DailyIDs != null)
		{
			foreach (string dailyID in m_DailyIDs)
			{
				m_DailyManager.SubscribeCollect(dailyID, Process);
				m_DailyManager.SubscribeRestore(dailyID, Process);
			}
		}
		
		Process();
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
		
		return productIDs?.Count(_ProductID => BadgeManager.IsProductUnread(_ProductID)) ?? 0;
	}

	int GetDailyCount()
	{
		List<string> dailyIDs = m_DailyManager.GetDailyIDs();
		
		return dailyIDs?.Count(_DailyID => m_DailyManager.IsDailyAvailable(_DailyID)) ?? 0;
	}
}
