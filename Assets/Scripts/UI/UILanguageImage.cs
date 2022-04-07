using UnityEngine;

public class UILanguageImage : UIEntity
{
	[SerializeField] WebGraphic m_Image;

	public void Setup(string _Language)
	{
		m_Image.Path = $"Thumbnails/Languages/{_Language}.jpg";
	}
}