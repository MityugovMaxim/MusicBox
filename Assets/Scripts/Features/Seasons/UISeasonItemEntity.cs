using Zenject;

public abstract class UISeasonItemEntity : UIEntity
{
	protected string SeasonID => m_SeasonID;

	protected string ItemID => m_ItemID;

	protected SeasonsManager SeasonsManager => m_SeasonsManager;

	[Inject] SeasonsManager m_SeasonsManager;

	string m_SeasonID;
	string m_ItemID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Setup(null, null);
	}

	public void Setup(string _SeasonID, string _ItemID)
	{
		if (m_SeasonID == _SeasonID && m_ItemID == _ItemID)
			return;
		
		if (!string.IsNullOrEmpty(m_SeasonID) && !string.IsNullOrEmpty(m_ItemID))
			Unsubscribe();
		
		m_SeasonID = _SeasonID;
		m_ItemID   = _ItemID;
		
		ProcessData();
		
		if (!string.IsNullOrEmpty(m_SeasonID) && !string.IsNullOrEmpty(m_ItemID))
			Subscribe();
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}