using UnityEngine;
using Zenject;

public class UISongBadge : UIEntity
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			m_SongsManager.Collection.Unsubscribe(DataEventType.Change, m_SongID, ProcessBadge);
			
			m_SongID = value;
			
			m_SongsManager.Collection.Subscribe(DataEventType.Change, m_SongID, ProcessBadge);
			
			ProcessBadge();
		}
	}

	[SerializeField] GameObject m_NewBadge;
	[SerializeField] GameObject m_HotBadge;
	[SerializeField] GameObject m_HardBadge;

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		SongID = null;
	}

	void ProcessBadge()
	{
		SongBadge badge = m_SongsManager.GetBadge(SongID);
		
		m_NewBadge.SetActive(badge == SongBadge.New);
		m_HotBadge.SetActive(badge == SongBadge.Hot);
		m_HardBadge.SetActive(badge == SongBadge.Hard);
	}
}
