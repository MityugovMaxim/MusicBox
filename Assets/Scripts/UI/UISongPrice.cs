using UnityEngine;
using Zenject;

public class UISongPrice : UIEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		long coins = m_SongsProcessor.GetPrice(m_SongID);
		
		m_Coins.Value = coins;
		
		gameObject.SetActive(coins != 0 && !m_ProfileProcessor.HasSong(m_SongID));
	}
}