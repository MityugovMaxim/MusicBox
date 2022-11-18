using TMPro;
using UnityEngine;
using Zenject;

public class UISongLabel : UIEntity
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			m_SongsManager.Collection.Unsubscribe(DataEventType.Change, m_SongID, ProcessLabel);
			
			m_SongID = value;
			
			m_SongsManager.Collection.Subscribe(DataEventType.Change, m_SongID, ProcessLabel);
			
			ProcessLabel();
		}
	}

	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Artist;

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		SongID = null;
	}

	void ProcessLabel()
	{
		m_Title.text  = m_SongsManager.GetTitle(SongID);
		m_Artist.text = m_SongsManager.GetArtist(SongID);
	}
}
