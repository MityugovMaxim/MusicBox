using Zenject;

public abstract class UISongEntity : UIEntity
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_SongID))
				Unsubscribe();
			
			m_SongID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_SongID))
				Subscribe();
		}
	}

	protected SongsManager SongsManager => m_SongsManager;

	string m_SongID;

	[Inject] SongsManager m_SongsManager;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		SongID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}
