using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIChestElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIChestElement> { }

	[SerializeField] UIChestImage  m_Image;
	[SerializeField] UIChestAction m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _ChestID)
	{
		m_Image.ChestID  = _ChestID;
		m_Action.ChestID = _ChestID;
		
		m_BadgeManager.ReadChest(_ChestID);
	}
}
