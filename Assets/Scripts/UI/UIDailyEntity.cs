using Zenject;

public abstract class UIDailyEntity : UIEntity
{
	public string DailyID
	{
		get => m_DailyID;
		set
		{
			if (m_DailyID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_DailyID))
				Unsubscribe();
			
			m_DailyID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_DailyID))
				Subscribe();
		}
	}

	protected DailyManager DailyManager => m_DailyManager;

	[Inject] DailyManager m_DailyManager;

	string m_DailyID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		DailyID = null;
	}

	public abstract void Subscribe();

	public abstract void Unsubscribe();

	public abstract void ProcessData();
}