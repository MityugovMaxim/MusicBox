using TMPro;
using UnityEngine;
using Zenject;

public class UIVoucherTitle : UIVoucherEntity
{
	[SerializeField] TMP_Text m_Amount;
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
		double amount = VouchersManager.GetAmount(VoucherID);
		
		m_Amount.text = amount > 0 ? $"-{amount:N0}%" : $"{amount:N0}%";
		
		m_Title.text = amount >= 0
			? m_Localization.Get("VOUCHER_BONUS")
			: m_Localization.Get("VOUCHER_DISCOUNT");
	}
}