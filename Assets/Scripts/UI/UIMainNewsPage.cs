using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMainNewsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.News;

	[SerializeField] RectTransform m_Container;

	[Inject] SignalBus       m_SignalBus;
	[Inject] NewsProcessor   m_NewsProcessor;
	[Inject] UINewsItem.Pool m_ItemPool;

	readonly List<UINewsItem> m_Items = new List<UINewsItem>();

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
		
		for (int i = m_Items.Count - 1; i >= 0; i--)
		{
			m_Items[i].Show(_Instant);
			
			if (!_Instant)
				await Task.Delay(150);
		}
	}
}
