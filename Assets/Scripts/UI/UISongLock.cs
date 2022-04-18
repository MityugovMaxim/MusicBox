using Zenject;

public class UISongLock : UIEntity
{
	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		gameObject.SetActive(m_SongsManager.IsSongLockedByLevel(m_SongID));
	}
}