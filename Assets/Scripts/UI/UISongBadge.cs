using UnityEngine;
using Zenject;

public class UISongBadge : UIEntity
{
	[SerializeField] GameObject m_NewBadge;
	[SerializeField] GameObject m_HotBadge;
	[SerializeField] GameObject m_HardBadge;

	[Inject] SongsProcessor m_SongsProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		SongBadge badge = m_SongsProcessor.GetBadge(m_SongID);
		
		m_NewBadge.SetActive(badge == SongBadge.New);
		m_HotBadge.SetActive(badge == SongBadge.Hot);
		m_HardBadge.SetActive(badge == SongBadge.Hard);
	}
}