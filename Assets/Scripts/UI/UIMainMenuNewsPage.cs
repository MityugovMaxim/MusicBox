using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuNewsPage : UIMainMenuPage
{
	const float LIST_SPACING = 30;

	public override MainMenuPageType Type => MainMenuPageType.News;

	[SerializeField] UILayout m_Content;

	[Inject] SignalBus       m_SignalBus;
	[Inject] NewsProcessor   m_NewsProcessor;
	[Inject] UINewsItem.Pool m_ItemPool;

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
		m_Content.Clear();
		
		List<string> newsIDs = m_NewsProcessor.GetNewsIDs();
		
		if (newsIDs == null || newsIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string newsID in newsIDs)
			m_Content.Add(new NewsItemEntity(newsID, m_ItemPool));
		
		m_Content.Space(LIST_SPACING);
		
		m_Content.Reposition();
	}
}
