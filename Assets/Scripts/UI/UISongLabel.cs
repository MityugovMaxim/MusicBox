using TMPro;
using UnityEngine;

public class UISongLabel : UISongEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Artist;

	protected override void Subscribe()
	{
		SongsManager.Collection.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SongsManager.Collection.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Title.text  = SongsManager.GetTitle(SongID);
		m_Artist.text = SongsManager.GetArtist(SongID);
	}
}
