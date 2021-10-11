using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UILevelItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Pool : MonoMemoryPool<UILevelItem> { }

	public string LevelID { get; private set; }

	[SerializeField] UILevelThumbnail m_Thumbnail;
	[SerializeField] UILevelDiscs     m_Discs;

	MenuProcessor m_MenuProcessor;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	public void Setup(string _LevelID)
	{
		LevelID = _LevelID;
		
		m_Thumbnail.Setup(LevelID);
		m_Discs.Setup(LevelID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>();
		
		if (levelMenu == null)
			return;
		
		levelMenu.Setup(LevelID);
		levelMenu.Show();
	}
}
