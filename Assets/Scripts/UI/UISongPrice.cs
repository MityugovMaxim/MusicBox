using UnityEngine;

public class UISongPrice : UISongEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	protected override void Subscribe()
	{
		SongsManager.Profile.Subscribe(DataEventType.Add, SongID, ProcessData);
		SongsManager.Profile.Subscribe(DataEventType.Remove, SongID, ProcessData);
		SongsManager.Collection.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SongsManager.Profile.Unsubscribe(DataEventType.Add, SongID, ProcessData);
		SongsManager.Profile.Unsubscribe(DataEventType.Remove, SongID, ProcessData);
		SongsManager.Collection.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		long coins = SongsManager.GetPrice(SongID);
		
		m_Coins.Value = coins;
		
		gameObject.SetActive(SongsManager.IsUnavailable(SongID) && coins > 0);
	}
}
