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
		
		m_Image.Path = m_NewsProcessor.GetImage(m_NewsID);
	}
}
