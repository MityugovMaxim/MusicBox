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

	string m_LevelID;

	LevelProcessor m_LevelProcessor;
	MenuProcessor  m_MenuProcessor;

	[Inject]
	public void Construct(
		LevelProcessor _LevelProcessor,
		MenuProcessor  _MenuProcessor
	)
	{
		m_LevelProcessor = _LevelProcessor;
		m_MenuProcessor  = _MenuProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		LevelBadge badge = m_LevelProcessor.GetBadge(m_LevelID);
		if (badge == LevelBadge.Hot)
		{
			m_HotBadge.Show();
			m_NewBadge.Hide();
		}
		else if (badge == LevelBadge.New)
		{
			m_NewBadge.Show();
			m_HotBadge.Hide();
		}
		else
		{
			m_NewBadge.Hide();
			m_HotBadge.Hide();
		}
		
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
