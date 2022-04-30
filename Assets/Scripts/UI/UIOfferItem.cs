using System;
using AudioBox.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UIOfferItem : UIGroupLayout
{
	[Preserve]
	public class Pool : UIEntityPool<UIOfferItem>
	{
		protected override void OnDespawned(UIOfferItem _Item)
		{
			base.OnDespawned(_Item);
			
			_Item.Hide(true);
		}
	}

	[SerializeField] UIOfferImage m_Image;
	[SerializeField] TMP_Text     m_Title;
	[SerializeField] TMP_Text     m_Description;
	[SerializeField] TMP_Text     m_Label;
	[SerializeField] Button       m_Button;

	[Inject] OffersProcessor       m_OffersProcessor;
	[Inject] OffersManager         m_OffersManager;
	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] LocalizationProcessor m_LocalizationProcessor;

	string         m_OfferID;
	Action<string> m_Process;

	public void Setup(string _OfferID, Action<string> _Process = null)
	{
		m_OfferID = _OfferID;
		m_Process = _Process;
		
		m_Image.Setup(m_OfferID);
		
		m_Title.text       = m_OffersProcessor.GetTitle(m_OfferID);
		m_Description.text = m_OffersProcessor.GetDescription(m_OfferID);
		
		ProcessLabel();
		
		ProcessButton();
	}

	public async void Settings()
	{
		UIOfferSettingsMenu offerSettingsMenu = m_MenuProcessor.GetMenu<UIOfferSettingsMenu>();
		
		offerSettingsMenu.Setup(m_OfferID);
		
		await m_MenuProcessor.Show(MenuType.OfferSettingsMenu);
	}

	public void Remove()
	{
		m_OffersProcessor.RemoveSnapshot(m_OfferID);
	}

	public void MoveUp()
	{
		m_OffersProcessor.MoveSnapshot(m_OfferID, -1);
	}

	public void MoveDown()
	{
		m_OffersProcessor.MoveSnapshot(m_OfferID, 1);
	}

	public void Process()
	{
		m_Process?.Invoke(m_OfferID);
	}

	void ProcessLabel()
	{
		int progress = m_OffersManager.GetProgress(m_OfferID);
		int target   = m_OffersManager.GetTarget(m_OfferID);
		
		if (m_OffersManager.IsCollected(m_OfferID))
			m_Label.text = m_LocalizationProcessor.Get("OFFER_COLLECTED");
		else if (progress < target)
			m_Label.text = m_LocalizationProcessor.Format("OFFER_PROGRESS", progress, target);
		else
			m_Label.text = m_LocalizationProcessor.Get("OFFER_COLLECT");
	}

	void ProcessButton()
	{
		m_Button.interactable = m_Process != null;
	}
}
