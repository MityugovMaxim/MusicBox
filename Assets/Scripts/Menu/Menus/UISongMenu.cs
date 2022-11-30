using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[Menu(MenuType.SongMenu)]
public class UISongMenu : UISlideMenu
{
	[SerializeField] UISongBackground m_Background;
	[SerializeField] UISongImage      m_Image;
	[SerializeField] UISongDiscs      m_Discs;
	[SerializeField] UISongLabel      m_Label;
	[SerializeField] SongPreview      m_Preview;
	[SerializeField] UISongQRCode     m_QR;
	[SerializeField] UISongPlay       m_Play;
	[SerializeField] UISongDownload   m_Download;

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	public void Next()
	{
		string songID = GetSongID(1);
		
		Setup(songID);
		
		m_Preview.Play(songID);
	}

	public void Previous()
	{
		string songID = GetSongID(-1);
		
		Setup(songID);
		
		m_Preview.Play(songID);
	}

	public void ToggleQR()
	{
		if (m_QR.Shown)
		{
			m_QR.Hide();
		}
		else
		{
			m_QR.Setup(m_SongID);
			m_QR.Show();
		}
	}

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Background.SongID = m_SongID;
		m_Image.SongID      = m_SongID;
		m_Discs.SongID      = m_SongID;
		m_Label.SongID      = m_SongID;
		m_Download.SongID   = m_SongID;
		m_Play.SongID       = m_SongID;
		
		m_QR.Hide(true);
	}

	string GetSongID(int _Offset)
	{
		List<string> songIDs = m_SongsManager.GetAvailableSongIDs();
		
		int index = songIDs.IndexOf(m_SongID);
		
		if (index >= 0 && index < songIDs.Count)
			return songIDs[MathUtility.Repeat(index + _Offset, songIDs.Count)];
		
		if (songIDs.Count > 0)
			return songIDs.FirstOrDefault();
		
		return m_SongID;
	}

	protected override void OnShowFinished()
	{
		base.OnShowFinished();
		
		m_Preview.Play(m_SongID);
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		m_Preview.Stop();
	}

	protected override bool OnEscape()
	{
		Hide();
		
		return true;
	}
}
