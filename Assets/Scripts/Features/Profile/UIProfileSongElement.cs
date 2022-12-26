using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProfileSongElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIProfileSongElement> { }

	[SerializeField] GameObject  m_BestMode;
	[SerializeField] GameObject  m_WorstMode;
	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongRank  m_Rank;
	[SerializeField] UISongLabel m_Label;
	[SerializeField] UISongDiscs m_Discs;

	[Inject] MenuProcessor m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID, ProfileSongMode _Mode)
	{
		m_SongID = _SongID;
		
		m_BestMode.SetActive(_Mode == ProfileSongMode.Best);
		m_WorstMode.SetActive(_Mode == ProfileSongMode.Worst);
		
		m_Image.SongID = m_SongID;
		m_Rank.SongID  = m_SongID;
		m_Label.SongID = m_SongID;
		m_Discs.SongID = m_SongID;
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
