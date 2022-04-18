using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ResultMenuPageType
{
	Reward,
	Level,
	Control
}

[Menu(MenuType.ResultMenu)]
public class UIResultMenu : UIMenu
{
	[SerializeField] UISongBackground   m_Background;
	[SerializeField] UIResultMenuPage[] m_Pages;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Background.Setup(m_SongID);
		
		foreach (UIResultMenuPage page in m_Pages)
			page.Setup(m_SongID);
	}

	public Task Select(ResultMenuPageType _PageType, bool _Instant = false)
	{
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

	public void Play(ResultMenuPageType _PageType)
	{
		foreach (UIResultMenuPage page in m_Pages)
		{
			if (page.Type == _PageType)
				page.Play();
		}
	}

	protected override void OnShowStarted()
	{
		Select(ResultMenuPageType.Reward, true);
	}

	protected override void OnShowFinished()
	{
		Play(ResultMenuPageType.Reward);
	}

	protected override void OnHideFinished()
	{
		foreach (UIResultMenuPage page in m_Pages)
			page.Hide(true);
	}
}