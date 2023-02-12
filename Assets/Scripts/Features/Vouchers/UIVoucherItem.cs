using UnityEngine;

public class UIVoucherItem : UIEntity
{
	[SerializeField] UIVoucherTitle m_Title;
	[SerializeField] UIVoucherTimer m_Timer;

	public void Setup(string _VoucherID)
	{
		m_Title.VoucherID = _VoucherID;
		m_Timer.VoucherID = _VoucherID;
	}
}
