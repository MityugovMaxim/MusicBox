using TMPro;
using UnityEngine;
using Zenject;

public class UISongLabel : UIEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Artist;

	[Inject] SongsProcessor m_SongsProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Title.text  = m_SongsProcessor.GetTitle(m_SongID);
		m_Artist.text = m_SongsProcessor.GetArtist(m_SongID);
	}
}