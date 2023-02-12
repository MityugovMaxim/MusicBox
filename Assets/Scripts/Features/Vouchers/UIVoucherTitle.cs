using TMPro;
using UnityEngine;
using Zenject;

public class UIVoucherTitle : UIVoucherEntity
{
	[SerializeField] TMP_Text m_Title;

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
		VoucherType voucherType = VouchersManager.GetType(VoucherID);
		
		switch (voucherType)
		{
			case VoucherType.Product:
				m_Title.text = m_Localization.Get("VOUCHER_PRODUCT");
				break;
			case VoucherType.Song:
				m_Title.text = m_Localization.Get("VOUCHER_SONG");
				break;
			case VoucherType.Chest:
				m_Title.text = m_Localization.Get("VOUCHER_CHEST");
				break;
			case VoucherType.Season:
				m_Title.text = m_Localization.Get("VOUCHER_SEASONS");
				break;
			default:
				m_Title.text = string.Empty;
				break;
		}
	}
}
