using UnityEngine;

public class UISeasonItemSong : UISeasonItemEntity
{
	[SerializeField] GameObject m_Content;
	[SerializeField] UISongItem m_Song;

	protected override void Subscribe()
	{
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.Collection.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		string songID = SeasonsManager.GetSongID(SeasonID, Level, Mode);
		
		m_Content.SetActive(!string.IsNullOrEmpty(songID));
		
		m_Song.Setup(songID);
	}
}
