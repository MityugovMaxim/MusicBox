using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIOffersPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Offers;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UILoader      m_Loader;
	[SerializeField] UIGroup       m_LoaderGroup;
	[SerializeField] UIGroup       m_ItemsGroup;
	[SerializeField] UIGroup       m_ErrorGroup;

	SignalBus            m_SignalBus;
	OfferProcessor       m_OfferProcessor;
	UIOffersPageItem.Pool m_ItemPool;

	List<string> m_OfferIDs;

	readonly List<UIOffersPageItem> m_Items = new List<UIOffersPageItem>();

	[Inject]
	public void Construct(
		SignalBus            _SignalBus,
		OfferProcessor       _OfferProcessor,
		UIOffersPageItem.Pool _ItemPool
	)
	{
		m_SignalBus      = _SignalBus;
		m_OfferProcessor = _OfferProcessor;
		m_ItemPool       = _ItemPool;
	}

	public async void Reload(bool _Instant = false)
	{
		if (m_OfferProcessor.Loaded)
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
			int frame = Time.frameCount;
			
			await m_OfferProcessor.LoadOffers();
			
			Refresh();
			
			bool instant = frame == Time.frameCount;
			
			m_LoaderGroup.Hide(instant);
			m_ErrorGroup.Hide(instant);
			m_ItemsGroup.Show(instant);
			
		}
		catch
		{
			m_ItemsGroup.Hide();
			m_LoaderGroup.Hide();
			m_ErrorGroup.Show();
		}
	}

	protected override void OnShowStarted()
	{
		Reload();
		
		m_SignalBus.Subscribe<OfferDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_SignalBus.Unsubscribe<OfferDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UIOffersPageItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		m_OfferIDs = m_OfferProcessor.GetOfferIDs();
		
		if (m_OfferIDs == null || m_OfferIDs.Count == 0)
			return;
		
		foreach (string offerID in m_OfferIDs)
		{
			UIOffersPageItem item = m_ItemPool.Spawn();
			
			item.Setup(offerID);
			
			item.RectTransform.SetParent(m_Container, false);
			
			m_Items.Add(item);
		}
	}
}
