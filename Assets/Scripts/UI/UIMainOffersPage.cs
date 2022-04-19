using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMainOffersPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Offers;

	[SerializeField] RectTransform m_Container;

	[SerializeField, Sound] string m_CollectSound;

	[Inject] SignalBus        m_SignalBus;
	[Inject] OffersManager    m_OffersManager;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] HapticProcessor  m_HapticProcessor;
	[Inject] SoundProcessor   m_SoundProcessor;
	[Inject] UIOfferItem.Pool m_ItemPool;

	bool m_Processing;

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
				
				item.Setup(offerID, ProcessOffer);
				
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

	async void ProcessOffer(string _OfferID)
	{
		if (m_Processing)
			return;
		
		m_Processing = true;
		
		m_SignalBus.Unsubscribe<OffersDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		int progress = m_OffersManager.GetProgress(_OfferID);
		int target   = m_OffersManager.GetTarget(_OfferID);
		
		bool collect = progress < target;
		
		bool success = collect
			? await m_OffersManager.ProgressOffer(_OfferID)
			: await m_OffersManager.CollectOffer(_OfferID);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			if (collect)
			{
				m_HapticProcessor.Process(Haptic.Type.Success);
				m_SoundProcessor.Play(m_CollectSound);
			}
			
			Refresh();
		}
		else
		{
			if (collect)
				await OfferProgressRetry(_OfferID);
			else
				await OfferCollectRetry(_OfferID);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
		
		m_SignalBus.Subscribe<OffersDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		
		m_Processing = false;
	}

	Task OfferProgressRetry(string _OfferID)
	{
		return m_MenuProcessor.RetryLocalizedAsync(
			"offer_progress",
			"OFFER_PROGRESS_ERROR_TITLE",
			"OFFER_PROGRESS_ERROR_MESSAGE",
			() => ProcessOffer(_OfferID),
			() => { }
		);
	}

	Task OfferCollectRetry(string _OfferID)
	{
		return m_MenuProcessor.RetryLocalizedAsync(
			"offer_collect",
			"OFFER_COLLECT_ERROR_TITLE",
			"OFFER_COLLECT_ERROR_MESSAGE",
			() => ProcessOffer(_OfferID),
			() => { }
		);
	}
}
