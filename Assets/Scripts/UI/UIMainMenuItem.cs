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

	UILevelMenu m_LevelMenu;

	[Inject]
	public void Construct(UILevelMenu _LevelMenu)
	{
		m_LevelMenu = _LevelMenu;
	}

	public void Setup(string _LevelID)
	{
		LevelID = _LevelID;
		
		m_Thumbnail.Setup(LevelID);
		m_ScoreRank.Setup(LevelID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_LevelMenu.Setup(LevelID);
		m_LevelMenu.Show();
	}
}
