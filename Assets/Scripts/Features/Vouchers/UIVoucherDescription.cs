using TMPro;
using UnityEngine;
using Zenject;

public class UIVoucherDescription : UIVoucherEntity
{
	[SerializeField] TMP_Text m_Description;

	[Inject] Localization m_Localization;

	protected override void Subscribe()
	{
		VouchersManager.Collection.Subscribe(DataEventType.Change, VoucherID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		VouchersManager.Collection.Unsubscribe(DataEventType.Change, VoucherID, ProcessData);
	}

	protected override void ProcessData()
	{
		string description;
		switch (VouchersManager.GetType(VoucherID))
		{
			case VoucherType.ProductDiscount:
				description = m_Localization.Get("VOUCHER_PRODUCT");
				break;
			case VoucherType.SongDiscount:
				description = m_Localization.Get("VOUCHER_SONG");
				break;
			case VoucherType.ChestDiscount:
				description = m_Localization.Get("VOUCHER_CHEST");
				break;
			default:
				description = string.Empty;
				break;
		}
		m_Description.text = description;
	}
}