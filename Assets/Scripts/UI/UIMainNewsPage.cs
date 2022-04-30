using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UIMainNewsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.News;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UIEntity      m_Control;

	[Inject] SignalBus       m_SignalBus;
	[Inject] NewsProcessor   m_NewsProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] UINewsItem.Pool m_ItemPool;

	readonly List<UINewsItem> m_Items = new List<UINewsItem>();

	public void CreateNews()
	{
		m_NewsProcessor.CreateSnapshot();
		
		Refresh();
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_NewsProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload news failed.");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"upload_news",
				"Upload failed",
				message
			);
		}
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_NewsProcessor.Load();
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh(false);
		
		m_SignalBus.Subscribe<NewsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<NewsDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		Refresh(true);
	}

	async void Refresh(bool _Instant)
	{
		foreach (UINewsItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		List<string> newsIDs = m_NewsProcessor.GetNewsIDs();
		
		if (newsIDs == null || newsIDs.Count == 0)
			return;
		
		foreach (string newsID in newsIDs)
		{
			if (string.IsNullOrEmpty(newsID))
				continue;
			
			UINewsItem item = m_ItemPool.Spawn(m_Container);
			
			item.Setup(newsID);
			
			m_Items.Add(item);
		}
		
		m_Control.BringToFront();
		
		for (int i = m_Items.Count - 1; i >= 0; i--)
		{
			m_Items[i].Show(_Instant);
			
			if (!_Instant)
				await Task.Delay(150);
		}
	}
}
