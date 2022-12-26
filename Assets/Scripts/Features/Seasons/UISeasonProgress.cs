using UnityEngine;

public class UISeasonProgress : UISeasonEntity
{
	[SerializeField] RectTransform m_Content;
	[SerializeField] RectTransform m_Foreground;
	[SerializeField] RectTransform m_Thumb;
	[SerializeField] RectOffset    m_Padding;

	public void Reposition() => ProcessData();

	protected override void Subscribe()
	{
		SeasonsManager.Profile.Subscribe(DataEventType.Remove, SeasonID, ProcessData);
		SeasonsManager.Profile.Subscribe(DataEventType.Add, SeasonID, ProcessData);
		SeasonsManager.Profile.Subscribe(DataEventType.Change, SeasonID, ProcessData);
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.Profile.Subscribe(DataEventType.Remove, SeasonID, ProcessData);
		SeasonsManager.Profile.Subscribe(DataEventType.Add, SeasonID, ProcessData);
		SeasonsManager.Profile.Subscribe(DataEventType.Change, SeasonID, ProcessData);
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		int   minLevel = SeasonsManager.GetMinLevel(SeasonID);
		int   maxLevel = SeasonsManager.GetMaxLevel(SeasonID);
		int   level    = SeasonsManager.GetLevel(SeasonID);
		float progress = SeasonsManager.GetProgress(SeasonID, level);
		
		float phase = MathUtility.Remap01(level + progress, minLevel, maxLevel);
		
		Rect rect = GetLocalRect(m_Content.GetWorldRect());
		
		if (Mathf.Approximately(rect.height, 0))
			return;
		
		rect = m_Padding.Remove(rect);
		
		float value = Mathf.Lerp(rect.yMin, rect.yMax, phase);
		
		Vector2 offset   = m_Foreground.offsetMax;
		Vector2 position = m_Thumb.anchoredPosition;
		
		offset.y   = value;
		position.y = value;
		
		m_Foreground.offsetMax   = offset;
		m_Thumb.anchoredPosition = position;
	}
}
