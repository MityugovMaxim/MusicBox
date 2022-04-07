using UnityEngine;

public class UISongImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Path = $"Thumbnails/Songs/{m_SongID}.jpg";
	}
}