using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainNewsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.News;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UIGroup       m_ItemsGroup;
	[SerializeField] UIGroup       m_EmptyGroup;

	SignalBus       m_SignalBus;
	NewsProcessor   m_NewsProcessor;
	UINewsItem.Pool m_ItemPool;

	List<string> m_NewsIDs;

	readonly List<UINewsItem> m_Items = new List<UINewsItem>();

	[Inject]
	public void Construct(
		SignalBus       _SignalBus,
		NewsProcessor   _NewsProcessor,
		UINewsItem.Pool _ItemPool
	)
	{
		m_SignalBus     = _SignalBus;
		m_NewsProcessor = _NewsProcessor;
		m_ItemPool      = _ItemPool;
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<NewsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<NewsDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UINewsItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		m_NewsIDs = m_NewsProcessor.GetNewsIDs();
		
		if (m_NewsIDs == null || m_NewsIDs.Count == 0)
		{
			m_ItemsGroup.Hide();
			m_EmptyGroup.Show();
			return;
		}
		
		m_ItemsGroup.Show();
		m_EmptyGroup.Hide();
		
		foreach (string newsID in m_NewsIDs)
		{
			UINewsItem item = m_ItemPool.Spawn();
			
			item.Setup(newsID);
			
			item.RectTransform.SetParent(m_Container, false);
			
			m_Items.Add(item);
		}
	}
}
