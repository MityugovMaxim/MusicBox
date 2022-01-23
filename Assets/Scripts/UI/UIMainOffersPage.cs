using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMainOffersPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Offers;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UIGroup       m_ItemsGroup;
	[SerializeField] UIGroup       m_EmptyGroup;

	SignalBus        m_SignalBus;
	ProfileProcessor m_ProfileProcessor;
	UIOfferItem.Pool m_ItemPool;

	List<string> m_OfferIDs;

	readonly List<UIOfferItem> m_Items = new List<UIOfferItem>();

	[Inject]
	public void Construct(
		SignalBus        _SignalBus,
		ProfileProcessor _ProfileProcessor,
		UIOfferItem.Pool _ItemPool
	)
	{
		m_SignalBus        = _SignalBus;
		m_ProfileProcessor = _ProfileProcessor;
		m_ItemPool         = _ItemPool;
	}

	protected override void OnShowStarted()
	{
		Refresh(false);
		
		m_SignalBus.Subscribe<OfferDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<OfferDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		Refresh(true);
	}

	async void Refresh(bool _Instant)
	{
		foreach (UIOfferItem item in m_Items)
		{
			item.Hide(true);
			m_ItemPool.Despawn(item);
		}
		m_Items.Clear();
		
		m_OfferIDs = m_ProfileProcessor.GetVisibleOfferIDs();
		
		if (m_OfferIDs == null || m_OfferIDs.Count == 0)
		{
			m_ItemsGroup.Hide();
			m_EmptyGroup.Show();
			return;
		}
		
		m_ItemsGroup.Show();
		m_EmptyGroup.Hide();
		
		foreach (string offerID in m_OfferIDs)
		{
			UIOfferItem item = m_ItemPool.Spawn();
			
			item.Setup(offerID);
			
			item.RectTransform.SetParent(m_Container, false);
			
			m_Items.Add(item);
		}
		
		for (int i = m_Items.Count - 1; i >= 0; i--)
		{
			m_Items[i].Show(_Instant);
			
			if (!_Instant)
				await Task.Delay(150);
		}
	}
}
