using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

public class UISeasonHeaderElement : UISeasonEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISeasonHeaderElement> { }

	[SerializeField] TMP_Text      m_Title;
	[SerializeField] UIAnalogTimer m_Timer;

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
		m_Title.text = SeasonsManager.GetTitle(SeasonID);
		m_Timer.Setup(SeasonsManager.GetEndTimestamp(SeasonID));
	}
}