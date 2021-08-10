using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UIMainMenuItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Factory : PlaceholderFactory<UIMainMenuItem, UIMainMenuItem> { }

	public string LevelID { get; private set; }

	[SerializeField] UILevelPreviewThumbnail m_Thumbnail;
	[SerializeField] UIScoreRank             m_ScoreRank;

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
		m_ScoreRank.Setup(LevelID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>(MenuType.LevelMenu);
		
		if (levelMenu == null)
			return;
		
		levelMenu.Setup(LevelID);
		levelMenu.Show();
	}
}
