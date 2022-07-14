using UnityEngine;

public class UIDailyImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	public void Setup()
	{
		m_Image.Path = "Thumbnails/Daily/daily.jpg";
	}
}