using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMainOffersPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Offers;

	[SerializeField] RectTransform m_Container;

	[Inject] SignalBus        m_SignalBus;
	[Inject] OffersManager    m_OffersManager;
	[Inject] UIOfferItem.Pool m_ItemPool;

	readonly List<UIOfferItem> m_Items = new List<UIOfferItem>();

	protected override void OnShowStarted()
	{
		Refresh(false);
		
		m_SignalBus.Subscribe<OffersDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<OffersDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		Refresh(true);
	}

	async void Refresh(bool _Instant)
	{
		foreach (UIOfferItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		List<string> availableOfferIDs = m_OffersManager.GetAvailableOfferIDs();
		
		if (availableOfferIDs != null)
		{
			foreach (string offerID in availableOfferIDs)
			{
				if (string.IsNullOrEmpty(offerID))
					continue;
				
				UIOfferItem item = m_ItemPool.Spawn(m_Container);
				
				item.Setup(offerID);
				
				m_Items.Add(item);
			}
		}
		
		List<string> collectedOfferIDs = m_OffersManager.GetCollectedOfferIDs();
		
		if (collectedOfferIDs != null)
		{
			foreach (string offerID in collectedOfferIDs)
			{
				if (string.IsNullOrEmpty(offerID))
					continue;
				
				UIOfferItem item = m_ItemPool.Spawn(m_Container);
				
				item.Setup(offerID);
				
				m_Items.Add(item);
			}
		}
		
		for (int i = m_Items.Count - 1; i >= 0; i--)
		{
			m_Items[i].Show(_Instant);
			
			if (!_Instant)
				await Task.Delay(150);
		}
	}
}
