using Zenject;

public abstract class UISeasonEntity : UIEntity
{
	public string SeasonID
	{
		get => m_SeasonID;
		set
		{
			if (m_SeasonID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_SeasonID))
				Unsubscribe();
			
			m_SeasonID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_SeasonID))
				Subscribe();
		}
	}

	protected SeasonsManager SeasonsManager => m_SeasonsManager;

	[Inject] SeasonsManager m_SeasonsManager;

	string m_SeasonID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		SeasonID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}