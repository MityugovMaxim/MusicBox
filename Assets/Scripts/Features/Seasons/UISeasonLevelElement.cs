using UnityEngine;
using UnityEngine.Scripting;

public class UISeasonLevelElement : UISeasonLevelEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISeasonLevelElement> { }

	[SerializeField] UISeasonLevelLabel   m_Level;
	[SerializeField] UISeasonLevelState[] m_State;
	[SerializeField] UISeasonItem         m_FreeItem;
	[SerializeField] UISeasonItem         m_PaidItem;

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
		m_Level.Setup(SeasonID, Level);
		m_FreeItem.Setup(SeasonID, Level);
		m_PaidItem.Setup(SeasonID, Level);
		
		foreach (UISeasonLevelState state in m_State)
			state.Setup(SeasonID, Level);
	}
}
