using TMPro;
using UnityEngine;

public class UINewsLabel : UINewsEntity
{
	[SerializeField] TMP_Text    m_Title;
	[SerializeField] TMP_Text    m_Description;
	[SerializeField] UIUnitLabel m_Date;

	protected override void Subscribe()
	{
		NewsManager.Descriptor.Subscribe(DataEventType.Add, NewsID, ProcessData);
		NewsManager.Descriptor.Subscribe(DataEventType.Remove, NewsID, ProcessData);
		NewsManager.Descriptor.Subscribe(DataEventType.Change, NewsID, ProcessData);
		NewsManager.Collection.Subscribe(DataEventType.Change, NewsID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		NewsManager.Descriptor.Unsubscribe(DataEventType.Add, NewsID, ProcessData);
		NewsManager.Descriptor.Unsubscribe(DataEventType.Remove, NewsID, ProcessData);
		NewsManager.Descriptor.Unsubscribe(DataEventType.Change, NewsID, ProcessData);
		NewsManager.Collection.Unsubscribe(DataEventType.Change, NewsID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Title.text       = NewsManager.GetTitle(NewsID);
		m_Description.text = NewsManager.GetDescription(NewsID);
		m_Date.Value       = NewsManager.GetTimestamp(NewsID);
	}
}
