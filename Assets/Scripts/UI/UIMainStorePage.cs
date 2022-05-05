using System;
using System.Collections.Generic;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UIMainStorePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Store;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UIEntity      m_Control;

	[Inject] SignalBus           m_SignalBus;
	[Inject] ProductsProcessor   m_ProductsProcessor;
	[Inject] MenuProcessor       m_MenuProcessor;
	[Inject] UIProductGroup.Pool m_ItemPool;

	List<string> m_ProductIDs;

	readonly List<UIProductGroup> m_Items = new List<UIProductGroup>();

	public void CreateProduct()
	{
		m_ProductsProcessor.CreateSnapshot();
		
		Refresh();
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_ProductsProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload products failed.");
			
			await m_MenuProcessor.ExceptionAsync("Upload failed", exception);
		}
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_ProductsProcessor.Load();
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UIProductGroup item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		Create();
		
		m_Control.BringToFront();
	}

	void Create()
	{
		List<string> productIDs = m_ProductsProcessor.GetProductIDs();
		
		CreateItems(string.Empty, productIDs);
	}

	void CreateItems(string _Title, ICollection<string> _ProductIDs)
	{
		if (_ProductIDs == null || _ProductIDs.Count == 0)
			return;
		
		UIProductGroup item = m_ItemPool.Spawn(m_Container);
		
		item.Setup(_Title, _ProductIDs);
		
		m_Items.Add(item);
	}
}
