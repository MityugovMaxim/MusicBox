using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UISongItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Pool : UIEntityPool<UISongItem> { }

	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongDiscs m_Discs;
	[SerializeField] UISongLock  m_Lock;
	[SerializeField] UISongBadge m_Badge;

	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		m_Discs.Setup(m_SongID);
		m_Lock.Setup(m_SongID);
		m_Badge.Setup(m_SongID);
	}

	public void Remove()
	{
		m_SongsProcessor.RemoveSnapshot(m_SongID);
	}

	public void MoveUp()
	{
		m_SongsProcessor.MoveSnapshot(m_SongID, -1);
	}

	public void MoveDown()
	{
		m_SongsProcessor.MoveSnapshot(m_SongID, 1);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_StatisticProcessor.LogMainMenuSongsPageItemClick(m_SongID);
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		if (songMenu == null)
			return;
		
		songMenu.Setup(m_SongID);
		songMenu.Show();
	}
}
