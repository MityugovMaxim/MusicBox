using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UISongElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UISongElement> { }

	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongLabel m_Label;
	[SerializeField] UISongDiscs m_Discs;
	[SerializeField] UISongBadge m_Badge;
	[SerializeField] UISongPlay  m_Play;

	[Inject] MenuProcessor m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.SongID = m_SongID;
		m_Label.SongID = m_SongID;
		m_Badge.SongID = m_SongID;
		m_Discs.SongID = m_SongID;
		
		m_Play.Setup(m_SongID);
	}

	protected override async void OnClick()
	{
		base.OnClick();
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		if (songMenu != null)
			songMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.SongMenu);
	}
}
