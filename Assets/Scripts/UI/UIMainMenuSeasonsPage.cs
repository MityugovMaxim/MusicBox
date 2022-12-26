using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuSeasonsPage : UIMainMenuPage
{
	const float LIST_SPACING = 0;

	public override MainMenuPageType Type => MainMenuPageType.Seasons;

	[SerializeField] UIVerticalScrollView  m_Scroll;
	[SerializeField] UILayout              m_Content;
	[SerializeField] UISeasonHeaderElement m_Header;
	[SerializeField] UISeasonProgress      m_Progress;
	[SerializeField] UIGroup               m_ContentGroup;
	[SerializeField] UIGroup               m_LoaderGroup;

	[Inject] SeasonsManager m_SeasonsManager;

	[Inject] UISeasonLevelElement.Pool m_LevelsPool;

	protected override async void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		bool instant = await m_SeasonsManager.Activate();
		
		Refresh();
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
		
		m_SeasonsManager.Collection.Subscribe(DataEventType.Add, Refresh);
		m_SeasonsManager.Collection.Subscribe(DataEventType.Remove, Refresh);
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Add, Refresh);
		m_SeasonsManager.Collection.Unsubscribe(DataEventType.Remove, Refresh);
	}

	void Refresh()
	{
		string seasonID = m_SeasonsManager.GetSeasonID();
		
		m_Header.SeasonID = seasonID;
		
		m_Content.Clear();
		
		CreateSeason();
		
		m_Content.Reposition();
		
		m_Progress.SeasonID = seasonID;
		
		SelectLevel(seasonID);
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
		
		foreach (int level in levels)
			m_Content.Add(new SeasonLevelElementEntity(seasonID, level, m_LevelsPool));
		
		VerticalStackLayout.End(m_Content);
	}

	void SelectLevel(string _SeasonID)
	{
		if (string.IsNullOrEmpty(_SeasonID))
			return;
		
		int level = m_SeasonsManager.GetAvailableLevel(_SeasonID);
		
		string levelID = $"{_SeasonID}_{level}";
		
		const TextAnchor alignment = TextAnchor.MiddleCenter;
		
		Vector2 position = m_Content.GetPosition(levelID, alignment);
		
		m_Scroll.Scroll(position, alignment);
	}
}
