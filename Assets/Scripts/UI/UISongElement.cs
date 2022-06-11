using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UISongElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UISongElement> { }

	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongLabel m_Label;
	[SerializeField] UISongDiscs m_Discs;
	[SerializeField] UISongLock  m_Lock;
	[SerializeField] UISongBadge m_Badge;
	[SerializeField] UISongPrice m_Price;
	[SerializeField] Button      m_PlayButton;

	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] ScoresProcessor    m_ScoresProcessor;
	[Inject] SongsManager       m_SongsManager;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		m_Discs.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		m_Lock.Setup(m_SongID);
		m_Badge.Setup(m_SongID);
		m_Price.Setup(m_SongID);
		
		ProcessPlay();
	}

	void ProcessPlay()
	{
		ScoreRank rank = m_ScoresProcessor.GetRank(m_SongID);
		
		m_PlayButton.gameObject.SetActive(rank == ScoreRank.None && m_SongsManager.IsSongAvailable(m_SongID));
		
		m_PlayButton.onClick.RemoveAllListeners();
		m_PlayButton.onClick.AddListener(Play);
	}

	void Play()
	{
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		if (songMenu == null)
			return;
		
		songMenu.Setup(m_SongID);
		songMenu.Play();
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