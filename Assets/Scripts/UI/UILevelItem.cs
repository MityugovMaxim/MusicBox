using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UILevelItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Pool : MonoMemoryPool<UILevelItem> { }

	[SerializeField] UILevelThumbnail m_Thumbnail;
	[SerializeField] UILevelDiscs     m_Discs;
	[SerializeField] UIGroup          m_NewBadge;
	[SerializeField] UIGroup          m_HotBadge;
	[SerializeField] GameObject       m_Lock;

	string m_LevelID;

	LevelManager   m_LevelManager;
	LevelProcessor m_LevelProcessor;
	MenuProcessor  m_MenuProcessor;

	[Inject]
	public void Construct(
		LevelManager   _LevelManager,
		LevelProcessor _LevelProcessor,
		MenuProcessor  _MenuProcessor
	)
	{
		m_LevelManager   = _LevelManager;
		m_LevelProcessor = _LevelProcessor;
		m_MenuProcessor  = _MenuProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		switch (m_LevelProcessor.GetBadge(m_LevelID))
		{
			case LevelBadge.Hot:
				m_HotBadge.Show();
				m_NewBadge.Hide();
				break;
			case LevelBadge.New:
				m_NewBadge.Show();
				m_HotBadge.Hide();
				break;
			default:
				m_NewBadge.Hide();
				m_HotBadge.Hide();
				break;
		}
		
		m_Lock.SetActive(m_LevelManager.IsLevelLockedByLevel(m_LevelID));
		
		m_Thumbnail.Setup(m_LevelID);
		m_Discs.Setup(m_LevelID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>();
		
		if (levelMenu == null)
			return;
		
		levelMenu.Setup(m_LevelID);
		levelMenu.Show();
	}
}
