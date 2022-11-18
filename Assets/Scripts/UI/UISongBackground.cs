using Zenject;

public class UISongBackground : UIBackground
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			m_SongsManager.Collection.Unsubscribe(DataEventType.Change, m_SongID, ProcessBackground);
			
			m_SongID = value;
			
			m_SongsManager.Collection.Subscribe(DataEventType.Change, m_SongID, ProcessBackground);
			
			ProcessBackground();
		}
	}

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		SongID = null;
	}

	void ProcessBackground()
	{
		string image = m_SongsManager.GetImage(SongID);
		
		Show(image);
	}
}
