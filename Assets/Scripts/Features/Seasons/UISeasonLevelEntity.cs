using Zenject;

public abstract class UISeasonLevelEntity : UIEntity
{
	public string SeasonID { get; private set; }

	public int Level { get; private set; }

	protected SeasonsManager SeasonsManager => m_SeasonsManager;

	[Inject] SeasonsManager m_SeasonsManager;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Setup(null, 0);
	}

	public void Setup(string _SeasonID, int _Level)
	{
		if (SeasonID == _SeasonID && Level == _Level)
			return;
		
		if (!string.IsNullOrEmpty(SeasonID))
			Unsubscribe();
		
		SeasonID = _SeasonID;
		Level    = _Level;
		
		ProcessData();
		
		if (!string.IsNullOrEmpty(SeasonID))
			Subscribe();
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}
