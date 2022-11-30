using UnityEngine;
using UnityEngine.UI;

public class UISeasonLevelProgress : UISeasonLevelEntity
{
	[SerializeField] GameObject m_Content;
	[SerializeField] Image      m_Progress;

	protected override void Subscribe()
	{
		SeasonsManager.Profile.Subscribe(DataEventType.Change, SeasonID, ProcessData);
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.Profile.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
		SeasonsManager.Collection.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Content.SetActive(Level > SeasonsManager.GetMinLevel(SeasonID));
		
		Vector2 anchor = m_Progress.rectTransform.anchorMin;
		
		anchor.y = SeasonsManager.GetProgress(SeasonID, Level);
		
		m_Progress.rectTransform.anchorMin = anchor;
	}
}
