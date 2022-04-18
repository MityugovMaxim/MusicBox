using UnityEngine;
using Zenject;

public class UISongImage : UIEntity
{
	[SerializeField] WebGraphic  m_Image;
	[SerializeField] UIGrayscale m_Album;
	[SerializeField] bool        m_Grayscale;

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Path = $"Thumbnails/Songs/{m_SongID}.jpg";
		
		if (m_Grayscale)
			m_Album.Grayscale = m_SongsManager.IsSongLockedByLevel(m_SongID) ? 0.975f : 0;
	}
}