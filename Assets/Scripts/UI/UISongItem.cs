using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UISongItem : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UISongItem> { }

	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongDiscs m_Discs;
	[SerializeField] UISongLock  m_Lock;
	[SerializeField] UISongBadge m_Badge;

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

	protected override void OnClick()
	{
		base.OnClick();
		
		m_StatisticProcessor.LogSongItemClick(m_SongID);
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		if (songMenu == null)
			return;
		
		songMenu.Setup(m_SongID);
		songMenu.Show();
	}
}
