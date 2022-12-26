using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UISongElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISongElement> { }

	[SerializeField] UISongImage  m_Image;
	[SerializeField] UISongLabel  m_Label;
	[SerializeField] UISongDiscs  m_Discs;
	[SerializeField] UISongRank   m_Rank;
	[SerializeField] UISongPrice  m_Price;
	[SerializeField] UISongPlay   m_Play;
	[SerializeField] UISongAction m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _SongID)
	{
		m_Image.SongID  = _SongID;
		m_Label.SongID  = _SongID;
		m_Rank.SongID   = _SongID;
		m_Discs.SongID  = _SongID;
		m_Price.SongID  = _SongID;
		m_Play.SongID   = _SongID;
		m_Action.SongID = _SongID;
		
		m_BadgeManager.ReadSong(_SongID);
	}
}
