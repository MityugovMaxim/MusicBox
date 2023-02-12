using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIFrameElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIFrameElement> { }

	[SerializeField] UIFrameImage  m_Image;
	[SerializeField] UIFrameAction m_Action;
	[SerializeField] UIFrameCheck  m_Check;

	public void Setup(string _FrameID)
	{
		m_Image.FrameID  = _FrameID;
		m_Action.FrameID = _FrameID;
		m_Check.FrameID  = _FrameID;
	}
}
