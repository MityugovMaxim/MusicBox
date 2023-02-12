using UnityEngine;

public class UIFrameImage : UIFrameEntity
{
	[SerializeField] WebGraphic m_Image;

	protected override void Subscribe()
	{
		FramesManager.Collection.Subscribe(DataEventType.Change, FrameID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		FramesManager.Collection.Unsubscribe(DataEventType.Change, FrameID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Image.Path = FramesManager.GetImage(FrameID);
	}
}
