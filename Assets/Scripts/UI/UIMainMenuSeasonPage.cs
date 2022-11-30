using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuSeasonPage : UIMainMenuPage
{
	const float LIST_SPACING = 30;

	public override MainMenuPageType Type => MainMenuPageType.Season;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] SeasonsManager m_SeasonsManager;

	[Inject] UISeasonHeaderElement.Pool m_SeasonHeaderPool;
	[Inject] UISeasonLevelElement.Pool  m_SeasonLevelPool;

	protected override async void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		bool instant = await m_SeasonsManager.Activate();
		
		Refresh();
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
	}

	void Refresh()
	{
		m_Content.Clear();
		
		CreateSeason();
		
		m_Content.Reposition();
	}

	void CreateSeason()
	{
		string seasonID = m_SeasonsManager.GetSeasonID();
		
		if (string.IsNullOrEmpty(seasonID))
			return;
		
		List<int> levels = m_SeasonsManager.GetLevels(seasonID);
		
		if (levels == null || levels.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new SeasonHeaderElementEntity(seasonID, m_SeasonHeaderPool));
		
		foreach (int level in levels)
			m_Content.Add(new SeasonLevelElementEntity(seasonID, level, m_SeasonLevelPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}
}
