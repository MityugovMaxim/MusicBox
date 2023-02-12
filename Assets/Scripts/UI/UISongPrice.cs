using UnityEngine;
using Zenject;

public class UISongPrice : UISongEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	[Inject] VouchersManager m_VouchersManager;

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
		
		m_Coins.Value = m_VouchersManager.GetSongDiscount(SongID, coins);
		
		gameObject.SetActive(SongsManager.IsPaid(SongID) && coins > 0);
	}
}
