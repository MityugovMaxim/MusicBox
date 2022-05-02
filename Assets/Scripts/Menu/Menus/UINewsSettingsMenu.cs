using System;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.NewsSettingsMenu)]
public class UINewsSettingsMenu : UIMenu
{
	[SerializeField] ScrollRect         m_Scroll;
	[SerializeField] UIImageField       m_Image;
	[SerializeField] UISerializedObject m_Fields;

	[Inject] NewsProcessor     m_NewsProcessor;
	[Inject] NewsDescriptor    m_NewsDescriptor;
	[Inject] LanguageProcessor m_LanguageProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	string       m_NewsID;
	NewsSnapshot m_Snapshot;
	Descriptor   m_Descriptor;

	public void Setup(string _NewsID)
	{
		m_NewsID = _NewsID;
		
		m_Scroll.verticalNormalizedPosition = 1;
		
		m_Image.Label = "Image";
		m_Image.Setup($"Thumbnails/News/{m_NewsID}.jpg");
		
		m_Fields.Clear();
		
		m_Snapshot = m_NewsProcessor.GetSnapshot(m_NewsID);
		
		if (m_Snapshot == null)
			return;
		
		m_Fields.Add($"News: {m_NewsID}", m_Snapshot);
		
		m_Descriptor = m_NewsDescriptor.GetDescriptor(m_NewsID);
		
		if (m_Descriptor == null)
			m_Descriptor = m_NewsDescriptor.CreateDescriptor(m_NewsID);
		
		if (m_Descriptor == null)
			return;
		
		m_Fields.Add($"Descriptor [{m_LanguageProcessor.Language}]", m_Descriptor);
	}

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.NewsSettingsMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_NewsProcessor.Load();
		
		Setup(m_NewsID);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			m_Descriptor.Setup(m_Snapshot.ID);
			
			await m_NewsProcessor.Upload(m_Snapshot.ID);
			
			await m_NewsDescriptor.Upload(m_Snapshot.ID);
			
			Setup(m_Snapshot.ID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload news failed.");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"news_upload",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}