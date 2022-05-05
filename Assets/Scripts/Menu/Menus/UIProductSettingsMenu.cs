using System;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ProductSettingsMenu)]
public class UIProductSettingsMenu : UIMenu
{
	[SerializeField] ScrollRect         m_Scroll;
	[SerializeField] UIImageField       m_Image;
	[SerializeField] UISerializedObject m_Fields;

	[Inject] ProductsProcessor  m_ProductsProcessor;
	[Inject] ProductsDescriptor m_ProductsDescriptor;
	[Inject] LanguageProcessor  m_LanguageProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;

	string          m_ProductID;
	ProductSnapshot m_Snapshot;
	Descriptor      m_Descriptor;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Scroll.verticalNormalizedPosition = 1;
		
		m_Image.Label = "Image";
		m_Image.Setup($"Thumbnails/Products/{m_ProductID}.jpg");
		
		m_Fields.Clear();
		
		m_Snapshot = m_ProductsProcessor.GetSnapshot(m_ProductID);
		
		if (m_Snapshot == null)
			return;
		
		m_Fields.Add($"Product: {m_ProductID}", m_Snapshot);
		
		m_Descriptor = m_ProductsDescriptor.GetDescriptor(m_ProductID);
		
		if (m_Descriptor == null)
			m_Descriptor = m_ProductsDescriptor.CreateDescriptor(m_ProductID);
		
		if (m_Descriptor == null)
			return;
		
		m_Fields.Add($"Descriptor [{m_LanguageProcessor.Language}]", m_Descriptor);
	}

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.ProductSettingsMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_ProductsProcessor.Load();
		
		Setup(m_ProductID);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			m_Descriptor.Setup(m_Snapshot.ID);
			
			await m_ProductsProcessor.Upload(m_Snapshot.ID);
			
			await m_ProductsDescriptor.Upload(m_Snapshot.ID);
			
			Setup(m_Snapshot.ID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload product failed. Product ID: '{0}'.", m_ProductID);
			
			await m_MenuProcessor.ExceptionAsync("Upload failed", exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}