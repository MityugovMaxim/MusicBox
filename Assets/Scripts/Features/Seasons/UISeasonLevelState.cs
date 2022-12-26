using UnityEngine;

public class UISeasonLevelState : UISeasonLevelEntity
{
	[SerializeField] GameObject m_StateActive;
	[SerializeField] GameObject m_StateInactive;

	protected override void Subscribe()
	{
		SeasonsManager.Profile.Subscribe(DataEventType.Add, SeasonID, ProcessData);
		SeasonsManager.Profile.Subscribe(DataEventType.Remove, SeasonID, ProcessData);
		SeasonsManager.Profile.Subscribe(DataEventType.Change, SeasonID, ProcessData);
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.Profile.Unsubscribe(DataEventType.Add, SeasonID, ProcessData);
		SeasonsManager.Profile.Unsubscribe(DataEventType.Remove, SeasonID, ProcessData);
		SeasonsManager.Profile.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
		SeasonsManager.Collection.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		if (m_StateActive != null)
			m_StateActive.SetActive(SeasonsManager.IsLevelAvailable(SeasonID, Level));
		
		if (m_StateInactive != null)
			m_StateInactive.SetActive(SeasonsManager.IsLevelUnavailable(SeasonID, Level));
	}
}
