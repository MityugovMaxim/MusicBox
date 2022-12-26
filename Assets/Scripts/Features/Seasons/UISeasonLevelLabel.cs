using UnityEngine;

public class UISeasonLevelLabel : UISeasonLevelEntity
{
	[SerializeField] UIUnitLabel m_LevelLeft;
	[SerializeField] UIUnitLabel m_LevelRight;

	protected override void Subscribe()
	{
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.Collection.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_LevelLeft.Value  = Level;
		m_LevelRight.Value = Level;
	}
}
