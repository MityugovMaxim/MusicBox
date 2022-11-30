using TMPro;
using UnityEngine;

public class UISeasonLevelLabel : UISeasonLevelEntity
{
	[SerializeField] TMP_Text m_Label;

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
		m_Label.text = Level.ToString();
	}
}