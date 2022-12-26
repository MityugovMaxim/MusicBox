using Zenject;

public abstract class UISeasonItemEntity : UIEntity
{
	protected string         SeasonID => m_SeasonID;
	protected int            Level    => m_Level;
	protected SeasonItemMode Mode     => m_Mode;

	protected SeasonsManager SeasonsManager => m_SeasonsManager;

	[Inject] SeasonsManager m_SeasonsManager;

	string         m_SeasonID;
	int            m_Level;
	SeasonItemMode m_Mode;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Setup(null, 0, SeasonItemMode.None);
	}

	public void Setup(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		if (m_SeasonID == _SeasonID && m_Level == _Level && m_Mode == _Mode)
			return;
		
		if (!string.IsNullOrEmpty(m_SeasonID))
			Unsubscribe();
		
		m_SeasonID = _SeasonID;
		m_Level    = _Level;
		m_Mode     = _Mode;
		
		ProcessData();
		
		if (!string.IsNullOrEmpty(m_SeasonID))
			Subscribe();
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}
