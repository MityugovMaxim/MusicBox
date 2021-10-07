using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UINewsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.News;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UILoader      m_Loader;
	[SerializeField] UIGroup       m_LoaderGroup;
	[SerializeField] UIGroup       m_ItemsGroup;
	[SerializeField] UIGroup       m_ErrorGroup;

	SignalBus           m_SignalBus;
	NewsProcessor       m_NewsProcessor;
	UINewsPageItem.Pool m_ItemPool;

	List<string> m_NewsIDs;

	readonly List<UINewsPageItem> m_Items = new List<UINewsPageItem>();

	[Inject]
	public void Construct(
		SignalBus           _SignalBus,
		NewsProcessor       _NewsProcessor,
		UINewsPageItem.Pool _ItemPool
	)
	{
		m_SignalBus     = _SignalBus;
		m_NewsProcessor = _NewsProcessor;
		m_ItemPool      = _ItemPool;
	}

	public async void Reload(bool _Instant = false)
	{
		if (m_NewsProcessor.Loaded)
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
			
			await m_NewsProcessor.LoadNews();
			
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
		
		m_SignalBus.Subscribe<NewsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_SignalBus.Unsubscribe<NewsDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UINewsPageItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		m_NewsIDs = m_NewsProcessor.GetNewsIDs();
		
		if (m_NewsIDs == null || m_NewsIDs.Count == 0)
			return;
		
		foreach (string newsID in m_NewsIDs)
		{
			UINewsPageItem item = m_ItemPool.Spawn();
			
			item.Setup(newsID);
			
			item.RectTransform.SetParent(m_Container, false);
			
			m_Items.Add(item);
		}
	}
}
