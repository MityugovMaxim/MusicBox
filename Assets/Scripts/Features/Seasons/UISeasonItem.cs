using UnityEngine;

public class UISeasonItem : UIEntity
{
	[SerializeField] SeasonItemMode      m_Mode;
	[SerializeField] UISeasonItemCoins   m_Coins;
	[SerializeField] UISeasonItemSong    m_Song;
	[SerializeField] UISeasonItemChest   m_Chest;
	[SerializeField] UISeasonItemVoucher m_Voucher;
	[SerializeField] UISeasonItemAction  m_Action;
	[SerializeField] UIHighlight         m_Highlight;

	public void Setup(string _SeasonID, int _Level)
	{
		m_Coins.Setup(_SeasonID, _Level, m_Mode);
		m_Song.Setup(_SeasonID, _Level, m_Mode);
		m_Chest.Setup(_SeasonID, _Level, m_Mode);
		m_Voucher.Setup(_SeasonID, _Level, m_Mode);
		m_Action.Setup(_SeasonID, _Level, m_Mode);
		
		if (m_Highlight != null)
			m_Highlight.Show();
	}
}
