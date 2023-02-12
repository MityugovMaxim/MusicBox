using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

public class UIStoreBadge : UIBadge
{
	[Inject] ProductsManager m_ProductsManager;
	[Inject] DailyManager    m_DailyManager;

	protected override void Subscribe()
	{
		BadgeManager.SubscribeProducts(Process);
		m_ProductsManager.Collection.Subscribe(DataEventType.Add, Process);
		m_ProductsManager.Collection.Subscribe(DataEventType.Remove, Process);
		m_ProductsManager.Profile.Subscribe(DataEventType.Add, Process);
		m_ProductsManager.Profile.Subscribe(DataEventType.Remove, Process);
		m_DailyManager.Collection.Subscribe(DataEventType.Add, Process);
		m_DailyManager.Collection.Subscribe(DataEventType.Remove, Process);
		m_DailyManager.Collection.Subscribe(DataEventType.Change, Process);
		m_DailyManager.Profile.Subscribe(Process);
		m_DailyManager.SubscribeCollect(Process);
		m_DailyManager.SubscribeStartTimer(Process);
		m_DailyManager.SubscribeEndTimer(Process);
		m_DailyManager.SubscribeCancelTimer(Process);
	}

	protected override void Unsubscribe()
	{
		BadgeManager.UnsubscribeProducts(Process);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Remove, Process);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Add, Process);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_DailyManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_DailyManager.Collection.Unsubscribe(DataEventType.Remove, Process);
		m_DailyManager.Collection.Unsubscribe(DataEventType.Change, Process);
		m_DailyManager.Profile.Unsubscribe(Process);
		m_DailyManager.UnsubscribeCollect(Process);
		m_DailyManager.UnsubscribeStartTimer(Process);
		m_DailyManager.UnsubscribeEndTimer(Process);
		m_DailyManager.UnsubscribeCancelTimer(Process);
	}

	protected override async void Preload()
	{
		await Task.WhenAll(
			m_ProductsManager.Activate(),
			m_DailyManager.Activate()
		);
		
		base.Preload();
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
