using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UISongItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISongItem> { }

	[SerializeField] UISongImage      m_Image;
	[SerializeField] UISongLabel      m_Label;
	[SerializeField] UISongDiscs      m_Discs;
	[SerializeField] UISongDifficulty m_Difficulty;
	[SerializeField] UISongAction     m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _SongID)
	{
		m_Image.SongID      = _SongID;
		m_Label.SongID      = _SongID;
		m_Discs.SongID      = _SongID;
		m_Difficulty.SongID = _SongID;
		m_Action.SongID     = _SongID;
		
		m_BadgeManager.Read(UISongsBadge.SONGS_GROUP, _SongID);
	}
}
