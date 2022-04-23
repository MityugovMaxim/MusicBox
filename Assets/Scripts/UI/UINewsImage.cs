using UnityEngine;

public class UINewsImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	string m_NewsID;

	public void Setup(string _NewsID)
	{
		m_NewsID = _NewsID;
		
		m_Image.Path = $"Thumbnails/News/{m_NewsID}.jpg";
	}
}