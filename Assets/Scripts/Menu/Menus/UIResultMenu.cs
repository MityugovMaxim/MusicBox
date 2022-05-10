using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public enum ResultMenuPageType
{
	Reward  = 0,
	Level   = 1,
	Control = 2,
}

[Menu(MenuType.ResultMenu)]
public class UIResultMenu : UIMenu
{
	[SerializeField] UISongBackground   m_Background;
	[SerializeField] UIResultMenuPage[] m_Pages;

	string             m_SongID;
	ResultMenuPageType m_PageType;

	public void Setup(string _SongID)
	{
		m_SongID   = _SongID;
		m_PageType = ResultMenuPageType.Reward;
		
		m_Background.Setup(m_SongID);
		
		foreach (UIResultMenuPage page in m_Pages)
			page.Setup(m_SongID);
	}

	public async void Next()
	{
		ResultMenuPageType pageType;
		switch (m_PageType)
		{
			case ResultMenuPageType.Reward:
				pageType = ResultMenuPageType.Level;
				break;
			
			case ResultMenuPageType.Level:
				pageType = ResultMenuPageType.Control;
				break;
			
			default:
				return;
		}
		
		UIResultMenuPage page = GetPage(pageType);
		
		if (page != null && page.Valid)
		{
			await SelectPage(pageType);
			
			page.Play();
		}
		else
		{
			m_PageType = pageType;
			
			Next();
		}
	}

	protected override void OnShowStarted()
	{
		SelectPage(m_PageType, true);
	}

	protected override void OnShowFinished()
	{
		UIResultMenuPage page = GetPage(m_PageType);
		
		page.Play();
	}

	protected override void OnHideFinished()
	{
		foreach (UIResultMenuPage page in m_Pages)
			page.Hide(true);
	}

	Task SelectPage(ResultMenuPageType _PageType, bool _Instant = false)
	{
		m_PageType = _PageType;
		
		List<Task> tasks = new List<Task>();
		foreach (UIResultMenuPage page in m_Pages)
		{
			if (page.Type == _PageType)
				tasks.Add(page.ShowAsync(_Instant));
			else
				tasks.Add(page.HideAsync(_Instant));
		}
		return Task.WhenAll(tasks);
	}

	UIResultMenuPage GetPage(ResultMenuPageType _PageType)
	{
		return m_Pages.FirstOrDefault(_Page => _Page.Type == _PageType);
	}
}