using UnityEngine;
using Zenject;

public class UISongImage : UIEntity
{
	[SerializeField] WebGraphic  m_Image;
	[SerializeField] UIGrayscale m_Album;

	[Inject] SongsManager m_SongsManager;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Path = $"Thumbnails/Songs/{m_SongID}.jpg";
		
		m_Album.Grayscale = m_SongsManager.IsSongAvailable(m_SongID) ? 0 : 0.975f;
	}
}