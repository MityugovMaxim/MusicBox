using UnityEngine;
using UnityEngine.Scripting;

public class UINewsItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UINewsItem> { }

	[SerializeField] UINewsImage  m_Image;
	[SerializeField] UINewsLabel  m_Label;
	[SerializeField] UINewsDate   m_Date;
	[SerializeField] UINewsAction m_Action;

	public void Setup(string _NewsID)
	{
		m_Image.NewsID  = _NewsID;
		m_Label.NewsID  = _NewsID;
		m_Date.NewsID   = _NewsID;
		m_Action.NewsID = _NewsID;
	}
}
