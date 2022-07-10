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

	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] ScoresProcessor m_ScoresProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		m_Badge.Setup(m_SongID);
		m_Discs.Setup(m_SongID);
		m_Play.Setup(m_SongID);
		
		ScoreRank rank = m_ScoresProcessor.GetRank(m_SongID);
		if (rank > ScoreRank.None)
		{
			m_Discs.gameObject.SetActive(true);
			m_Play.gameObject.SetActive(false);
		}
		else
		{
			m_Play.gameObject.SetActive(true);
			m_Discs.gameObject.SetActive(false);
		}
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