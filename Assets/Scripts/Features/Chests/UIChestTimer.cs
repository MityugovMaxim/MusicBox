using UnityEngine;

public class UIChestTimer : UIChestEntity
{
	[SerializeField] UIAnalogTimer m_Timer;
	[SerializeField] UIGroup       m_TimerGroup;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_TimerGroup.Hide(true);
	}

	protected override void Subscribe()
	{
		ChestsManager.Profile.Subscribe(DataEventType.Change, ChestID, ProcessTimer);
		ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessTimer);
	}

	protected override void Unsubscribe()
	{
		ChestsManager.Profile.Unsubscribe(DataEventType.Change, ChestID, ProcessTimer);
		ChestsManager.Collection.Unsubscribe(DataEventType.Change, ProcessTimer);
	}

	protected override void ProcessData()
	{
		if (ChestsManager.IsStarted(ChestID))
			m_TimerGroup.Show(true);
		else
			m_TimerGroup.Hide(true);
		
		m_Timer.Setup(ChestsManager.GetEndTimestamp(ChestID));
	}

	void ProcessTimer()
	{
		if (ChestsManager.IsStarted(ChestID))
			m_TimerGroup.Show();
		else
			m_TimerGroup.Hide();
		
		m_Timer.Setup(ChestsManager.GetEndTimestamp(ChestID));
	}
}
