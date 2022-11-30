using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UINewsElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UINewsElement> { }

	[SerializeField] UINewsImage  m_Image;
	[SerializeField] UINewsLabel  m_Label;
	[SerializeField] UINewsDate   m_Date;
	[SerializeField] UINewsAction m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _NewsID)
	{
		m_Image.NewsID  = _NewsID;
		m_Label.NewsID  = _NewsID;
		m_Date.NewsID   = _NewsID;
		m_Action.NewsID = _NewsID;
		
		m_BadgeManager.Read(UINewsBadge.NEWS_GROUP, _NewsID);
	}
}
