using UnityEngine;

public class UISongLock : UISongEntity
{
	[SerializeField] GameObject m_Content;

	protected override void Subscribe()
	{
		SongsManager.Profile.Subscribe(DataEventType.Add, SongID, ProcessData);
		SongsManager.Profile.Subscribe(DataEventType.Remove, SongID, ProcessData);
		SongsManager.Profile.Subscribe(DataEventType.Change, SongID, ProcessData);
		SongsManager.Collection.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SongsManager.Profile.Unsubscribe(DataEventType.Add, SongID, ProcessData);
		SongsManager.Profile.Unsubscribe(DataEventType.Remove, SongID, ProcessData);
		SongsManager.Profile.Unsubscribe(DataEventType.Change, SongID, ProcessData);
		SongsManager.Collection.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Content.SetActive(SongsManager.IsAvailable(SongID) || SongsManager.IsPaid(SongID));
	}
}
