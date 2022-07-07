using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UISongItem : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UISongItem> { }

	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongLabel m_Label;
	[SerializeField] UISongDiscs m_Discs;
	[SerializeField] UISongBadge m_Badge;

	[Inject] MenuProcessor m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		m_Discs.Setup(m_SongID);
		m_Badge.Setup(m_SongID);
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		if (songMenu == null)
			return;
		
		songMenu.Setup(m_SongID);
		songMenu.Show();
	}
}
