using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMainMenuOffersPage : UIMainMenuPage
{
	const float LIST_SPACING = 30;

	public override MainMenuPageType Type => MainMenuPageType.Offers;

	[SerializeField] UILayout m_Content;

	[SerializeField, Sound] string m_CollectSound;

	[Inject] SignalBus           m_SignalBus;
	[Inject] OffersManager       m_OffersManager;
	[Inject] RolesProcessor      m_RolesProcessor;
	[Inject] MenuProcessor       m_MenuProcessor;
	[Inject] HapticProcessor     m_HapticProcessor;
	[Inject] SoundProcessor      m_SoundProcessor;
	[Inject] UIAdminElement.Pool m_AdminPool;
	[Inject] UIOfferItem.Pool    m_ItemPool;

	bool m_Processing;

	protected override void OnShowStarted()
	{
		Refresh();
		
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
		m_Content.Clear();
		
		CreateAdmin();
		
		CreateAvailable();
		
		CreateCollected();
		
		m_Content.Reposition();
	}

	void CreateAdmin()
	{
		if (!m_RolesProcessor.HasOffersPermission())
			return;
		
		AdminElementEntity offers = new AdminElementEntity(
			"Edit offers",
			"offers",
			"offers_descriptors",
			typeof(OfferSnapshot),
			m_AdminPool
		);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(offers);
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateAvailable()
	{
		List<string> offerIDs = m_OffersManager.GetAvailableOfferIDs();
		
		if (offerIDs == null || offerIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string offerID in offerIDs)
			m_Content.Add(new OfferItemEntity(offerID, ProcessOffer, m_ItemPool));
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateCollected()
	{
		List<string> offerIDs = m_OffersManager.GetCollectedOfferIDs();
		
		if (offerIDs == null || offerIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string offerID in offerIDs)
			m_Content.Add(new OfferItemEntity(offerID, null, m_ItemPool));
		
		m_Content.Space(LIST_SPACING);
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
		
		bool collect = progress >= target;
		
		bool success = collect
			? await m_OffersManager.CollectOffer(_OfferID)
			: await m_OffersManager.ProgressOffer(_OfferID);
		
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
				await OfferCollectRetry(_OfferID);
			else
				await OfferProgressRetry(_OfferID);
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
			"main_menu",
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
			"main_menu",
			"OFFER_COLLECT_ERROR_TITLE",
			"OFFER_COLLECT_ERROR_MESSAGE",
			() => ProcessOffer(_OfferID),
			() => { }
		);
	}
}
