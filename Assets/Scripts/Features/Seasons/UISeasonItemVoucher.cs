using UnityEngine;

public class UISeasonItemVoucher : UISeasonItemEntity
{
	[SerializeField] GameObject    m_Content;
	[SerializeField] UIVoucherItem m_Voucher;

	protected override void Subscribe()
	{
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.Collection.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		string voucherID = SeasonsManager.GetVoucherID(SeasonID, Level, Mode);
		
		m_Content.SetActive(!string.IsNullOrEmpty(voucherID));
		
		m_Voucher.Setup(voucherID);
	}
}
