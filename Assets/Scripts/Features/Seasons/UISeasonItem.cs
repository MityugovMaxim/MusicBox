using UnityEngine;

public class UISeasonItem : UIEntity
{
	[SerializeField] UISeasonItemCoins   m_Coins;
	[SerializeField] UISeasonItemSong    m_Song;
	[SerializeField] UISeasonItemChest   m_Chest;
	[SerializeField] UISeasonItemVoucher m_Voucher;

	[SerializeField] UISeasonItemButton  m_Button;

	public void Setup(string _SeasonID, string _ItemID)
	{
		m_Coins.Setup(_SeasonID, _ItemID);
		m_Song.Setup(_SeasonID, _ItemID);
		m_Chest.Setup(_SeasonID, _ItemID);
		m_Voucher.Setup(_SeasonID, _ItemID);
		m_Button.Setup(_SeasonID, _ItemID);
	}
}
