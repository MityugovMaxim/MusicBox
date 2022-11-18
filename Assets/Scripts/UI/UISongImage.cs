using UnityEngine;
using Zenject;

public class UISongImage : UIEntity
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			m_SongsManager.Unsubscribe(DataEventType.Add, m_SongID, ProcessImage);
			m_SongsManager.Unsubscribe(DataEventType.Remove, m_SongID, ProcessImage);
			m_SongsManager.Collection.Unsubscribe(DataEventType.Change, m_SongID, ProcessImage);
			
			m_SongID = value;
			
			m_SongsManager.Subscribe(DataEventType.Add, m_SongID, ProcessImage);
			m_SongsManager.Subscribe(DataEventType.Remove, m_SongID, ProcessImage);
			m_SongsManager.Collection.Subscribe(DataEventType.Change, m_SongID, ProcessImage);
			
			ProcessImage();
		}
	}

	[SerializeField] WebGraphic  m_Image;
	[SerializeField] UIGrayscale m_Album;
	[SerializeField] bool        m_Grayscale;

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		SongID = null;
	}

	void ProcessImage()
	{
		m_Image.Path = m_SongsManager.GetImage(SongID);
		
		if (m_Grayscale)
			m_Album.Grayscale = m_SongsManager.IsSongLockedByLevel(SongID) ? 0.8f : 0;
	}
}
