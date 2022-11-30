using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuProfilePage : UIMainMenuPage
{
	const float LIST_SPACING = 15;
	const float GRID_SPACING = 6;

	public override MainMenuPageType Type => MainMenuPageType.Profile;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] FramesManager m_FramesManager;

	[Inject] UIProfileElement.Pool      m_ProfilePool;
	[Inject] UIProfileDiscsElement.Pool m_ProfileDiscsPool;
	[Inject] UIAmbientElement.Pool      m_AmbientPool;
	[Inject] UIFrameElement.Pool        m_FramesPool;

	protected override async void OnShowStarted()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		bool instant = await m_FramesManager.Activate();
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
		
		Refresh();
	}

	void Refresh()
	{
		m_Content.Clear();
		
		CreateContent();
		
		CreateFrames();
		
		m_Content.Reposition();
	}

	void CreateContent()
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new ProfileElementEntity(m_ProfilePool));
		
		m_Content.Space(LIST_SPACING);
		
		m_Content.Add(new ProfileDiscsElementEntity(m_ProfileDiscsPool));
		
		m_Content.Space(LIST_SPACING);
		
		m_Content.Add(new AmbientElementEntity(m_AmbientPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateFrames()
	{
		List<string> frameIDs = m_FramesManager.GetFrameIDs();
		
		if (frameIDs == null || frameIDs.Count < 2)
			return;
		
		int count = Mathf.Min(frameIDs.Count, 4);
		
		VerticalGridLayout.Start(m_Content, count, 1, GRID_SPACING, GRID_SPACING);
		
		foreach (string frameID in frameIDs)
		{
			if (string.IsNullOrEmpty(frameID))
				continue;
			
			m_Content.Add(new FrameElementEntity(frameID, m_FramesPool));
		}
		
		VerticalGridLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}
}
