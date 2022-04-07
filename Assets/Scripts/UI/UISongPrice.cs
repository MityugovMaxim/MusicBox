using TMPro;
using UnityEngine;
using Zenject;

public class UISongPrice : UIEntity
{
	[SerializeField] TMP_Text m_Label;

	string m_SongID;

	[Inject] SongsProcessor m_SongsProcessor;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		long price = m_SongsProcessor.GetPrice(m_SongID);
		
		m_Label.text = price.ToString();
	}
}