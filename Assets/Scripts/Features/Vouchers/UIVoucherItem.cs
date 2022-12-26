using UnityEngine;

public class UIVoucherItem : UIEntity
{
	[SerializeField] UIVoucherTitle       m_Title;
	[SerializeField] UIVoucherDescription m_Description;
	[SerializeField] UIVoucherTimer       m_Timer;

	public void Setup(string _VoucherID)
	{
		m_Title.VoucherID       = _VoucherID;
		m_Description.VoucherID = _VoucherID;
		m_Timer.VoucherID       = _VoucherID;
	}
}
