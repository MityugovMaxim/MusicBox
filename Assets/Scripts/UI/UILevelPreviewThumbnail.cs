using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelPreviewThumbnail : UIEntity
{
	[SerializeField] Image m_Thumbnail;

	LevelProcessor m_LevelProcessor;

	[Inject]
	public void Construct(LevelProcessor _LevelProcessor)
	{
		m_LevelProcessor = _LevelProcessor;
	}

	public void Setup(string _LevelID)
	{
		Sprite previewThumbnail = m_LevelProcessor.GetPreviewThumbnail(_LevelID);
		
		m_Thumbnail.sprite = previewThumbnail;
	}
}
