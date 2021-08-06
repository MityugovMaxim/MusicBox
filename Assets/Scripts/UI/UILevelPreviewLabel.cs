using TMPro;
using UnityEngine;
using Zenject;

public class UILevelPreviewLabel : UIEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Artist;

	LevelProcessor m_LevelProcessor;

	[Inject]
	public void Construct(LevelProcessor _LevelProcessor)
	{
		m_LevelProcessor = _LevelProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_Title.text  = m_LevelProcessor.GetTitle(_LevelID);
		m_Artist.text = m_LevelProcessor.GetArtist(_LevelID);
	}
}