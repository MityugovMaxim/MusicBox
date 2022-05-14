using UnityEngine;
using Zenject;

public class UISongPrice : UIEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	string m_SongID;

	[Inject] SongsProcessor m_SongsProcessor;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Coins.Value = m_SongsProcessor.GetPrice(m_SongID);
	}
}