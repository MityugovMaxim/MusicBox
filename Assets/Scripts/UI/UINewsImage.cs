using UnityEngine;
using Zenject;

public class UINewsImage : UIEntity
{
	[SerializeField] WebImage m_Image;

	[Inject] NewsProcessor m_NewsProcessor;

	string m_NewsID;

	public void Setup(string _NewsID)
	{
		m_NewsID = _NewsID;
		
		string image = m_NewsProcessor.GetImage(m_NewsID);
		
		m_Image.Path = $"Thumbnails/News/{image}.jpg";
	}
}