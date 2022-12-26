using UnityEngine;

public class UINewsImage : UINewsEntity
{
	[SerializeField] WebGraphic m_Image;

	protected override void Subscribe()
	{
		NewsManager.Collection.Subscribe(DataEventType.Change, NewsID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		NewsManager.Collection.Unsubscribe(DataEventType.Change, NewsID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Image.Path = NewsManager.GetImage(NewsID);
	}
}
