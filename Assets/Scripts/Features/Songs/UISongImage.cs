using UnityEngine;

public class UISongImage : UISongEntity
{
	[SerializeField] WebGraphic m_Image;

	protected override void Subscribe()
	{
		SongsManager.Collection.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SongsManager.Collection.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Image.Path = SongsManager.GetImage(SongID);
	}
}
