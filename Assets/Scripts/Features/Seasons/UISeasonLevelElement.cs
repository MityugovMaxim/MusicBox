using UnityEngine;
using UnityEngine.Scripting;

public class UISeasonLevelElement : UISeasonLevelEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISeasonLevelElement> { }

	[SerializeField] UISeasonLevelLabel    m_Label;
	[SerializeField] UISeasonLevelProgress m_Progress;
	[SerializeField] GameObject            m_FreeContent;
	[SerializeField] GameObject            m_PaidContent;
	[SerializeField] UISeasonItem          m_FreeItem;
	[SerializeField] UISeasonItem          m_PaidItem;

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
		string freeItemID = SeasonsManager.GetFreeItemID(SeasonID, Level);
		string paidItemID = SeasonsManager.GetPaidItemID(SeasonID, Level);
		
		m_Label.Setup(SeasonID, Level);
		
		m_Progress.Setup(SeasonID, Level);
		
		m_FreeContent.SetActive(!string.IsNullOrEmpty(freeItemID));
		
		m_PaidContent.SetActive(!string.IsNullOrEmpty(paidItemID));
		
		m_FreeItem.Setup(SeasonID, freeItemID);
		
		m_PaidItem.Setup(SeasonID, paidItemID);
	}
}
