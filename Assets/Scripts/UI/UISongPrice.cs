using UnityEngine;
using Zenject;

public class UISongPrice : UIEntity
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			m_SongsManager.Unsubscribe(DataEventType.Add, m_SongID, ProcessPrice);
			m_SongsManager.Unsubscribe(DataEventType.Remove, m_SongID, ProcessPrice);
			m_SongsManager.Collection.Unsubscribe(DataEventType.Change, m_SongID, ProcessPrice);
			
			m_SongID = value;
			
			m_SongsManager.Subscribe(DataEventType.Add, m_SongID, ProcessPrice);
			m_SongsManager.Subscribe(DataEventType.Remove, m_SongID, ProcessPrice);
			m_SongsManager.Collection.Subscribe(DataEventType.Change, m_SongID, ProcessPrice);
			
			ProcessPrice();
		}
	}

	[SerializeField] UIUnitLabel m_Coins;

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	void ProcessPrice()
	{
		long coins = m_SongsManager.GetPrice(SongID);
		
		m_Coins.Value = coins;
		
		gameObject.SetActive(coins > 0 && !m_SongsManager.Contains(SongID));
	}
}
