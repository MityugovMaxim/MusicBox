using System;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.OfferSettingsMenu)]
public class UIOfferSettingsMenu : UIMenu
{
	[SerializeField] ScrollRect         m_Scroll;
	[SerializeField] UIImageField       m_Image;
	[SerializeField] UISerializedObject m_Fields;

	[Inject] OffersProcessor   m_OffersProcessor;
	[Inject] OffersDescriptor  m_OffersDescriptor;
	[Inject] LanguageProcessor m_LanguageProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	string        m_OfferID;
	OfferSnapshot m_Snapshot;
	Descriptor    m_Descriptor;

	public void Setup(string _OfferID)
	{
		m_OfferID = _OfferID;
		
		m_Scroll.verticalNormalizedPosition = 1;
		
		m_Image.Label = "Image";
		m_Image.Setup($"Thumbnails/Offers/{m_OfferID}.jpg");
		
		m_Fields.Clear();
		
		m_Snapshot = m_OffersProcessor.GetSnapshot(m_OfferID);
		
		if (m_Snapshot == null)
			return;
		
		m_Fields.Add($"Offer: {m_OfferID}", m_Snapshot);
		
		m_Descriptor = m_OffersDescriptor.GetDescriptor(m_OfferID);
		
		if (m_Descriptor == null)
			m_Descriptor = m_OffersDescriptor.CreateDescriptor(m_OfferID);
		
		if (m_Descriptor == null)
			return;
		
		m_Fields.Add($"Descriptor [{m_LanguageProcessor.Language}]", m_Descriptor);
	}

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.OfferSettingsMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			m_Descriptor.Setup(m_Snapshot.ID);
			
			await m_OffersProcessor.Upload(m_OfferID);
			
			await m_OffersDescriptor.Upload(m_OfferID);
			
			Setup(m_Snapshot.ID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload offer failed. Offer ID: '{0}'.", m_OfferID);
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"offer_upload",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}