using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMainOffersPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Offers;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UILoader      m_Loader;
	[SerializeField] UIGroup       m_ItemsGroup;
	[SerializeField] UIGroup       m_LoaderGroup;
	[SerializeField] UIGroup       m_ErrorGroup;
	[SerializeField] UIGroup       m_EmptyGroup;

	SignalBus        m_SignalBus;
	OffersProcessor   m_OffersProcessor;
	UIOfferItem.Pool m_ItemPool;

	List<string> m_OfferIDs;

	readonly List<UIOfferItem> m_Items = new List<UIOfferItem>();

	[Inject]
	public void Construct(
		SignalBus            _SignalBus,
		OffersProcessor       _OffersProcessor,
		UIOfferItem.Pool _ItemPool
	)
	{
		m_SignalBus      = _SignalBus;
		m_OffersProcessor = _OffersProcessor;
		m_ItemPool       = _ItemPool;
	}

	public async void Reload(bool _Instant = false)
	{
		if (m_OffersProcessor.Loaded)
		{
			m_LoaderGroup.Hide(true);
			m_ErrorGroup.Hide(true);
			m_ItemsGroup.Show(true);
			Refresh();
			return;
		}
		
		m_ItemsGroup.Hide(_Instant);
		m_ErrorGroup.Hide(_Instant);
		m_LoaderGroup.Show(_Instant);
		
		m_Loader.Restore();
		m_Loader.Play();
		
		try
		{
			await m_OffersProcessor.LoadOffers();
			
			Refresh();
			
			m_LoaderGroup.Hide();
			m_ErrorGroup.Hide();
			m_ItemsGroup.Show();
			
		}
		catch
		{
			await Task.Delay(1500);
			
			m_ItemsGroup.Hide();
			m_LoaderGroup.Hide();
			m_ErrorGroup.Show();
		}
	}

	protected override void OnShowStarted()
	{
		Reload(true);
		
		m_SignalBus.Subscribe<OfferDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_SignalBus.Unsubscribe<OfferDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UIOfferItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		m_OfferIDs = m_OffersProcessor.GetOfferIDs();
		
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
	}
}
