using UnityEngine;
using Zenject;

public class UIChestTime : UIEntity
{
	public string ChestID
	{
		get => m_ChestID;
		set
		{
			if (m_ChestID == value)
				return;
			
			m_ChestsManager.Profile.Unsubscribe(DataEventType.Change, m_ChestID, ProcessTime);
			m_ChestsManager.Collection.Unsubscribe(DataEventType.Change, ProcessTime);
			
			m_ChestID = value;
			
			ProcessTime();
			
			m_ChestsManager.Profile.Subscribe(DataEventType.Change, m_ChestID, ProcessTime);
			m_ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessTime);
		}
	}

	[SerializeField] UIAnalogTimer m_Timer;

	[Inject] ChestsManager m_ChestsManager;

	string m_ChestID;

	void ProcessTime()
	{
		m_Timer.Timestamp = m_ChestsManager.GetOpenTime(ChestID);
	}
}
