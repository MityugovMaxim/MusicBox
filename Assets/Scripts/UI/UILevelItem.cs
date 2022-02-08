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
	[SerializeField] GameObject       m_NewBadge;
	[SerializeField] GameObject       m_HotBadge;
	[SerializeField] GameObject       m_Lock;

	string m_LevelID;

	LevelManager       m_LevelManager;
	LevelProcessor     m_LevelProcessor;
	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	[Inject]
	public void Construct(
		LevelManager       _LevelManager,
		LevelProcessor     _LevelProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_LevelManager       = _LevelManager;
		m_LevelProcessor     = _LevelProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		LevelBadge badge = m_LevelProcessor.GetBadge(m_LevelID);
		m_HotBadge.SetActive(badge == LevelBadge.Hot);
		m_NewBadge.SetActive(badge == LevelBadge.New);
		
		m_Lock.SetActive(m_LevelManager.IsLevelLockedByLevel(m_LevelID));
		
		m_Thumbnail.Setup(m_LevelID);
		m_Discs.Setup(m_LevelID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_StatisticProcessor.LogMainMenuLevelPageItemClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.Selection);
		
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>();
		
		if (levelMenu == null)
			return;
		
		levelMenu.Setup(m_LevelID);
		levelMenu.Show();
	}
}
