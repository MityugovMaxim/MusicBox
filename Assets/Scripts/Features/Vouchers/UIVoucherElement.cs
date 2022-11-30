using UnityEngine;
using UnityEngine.Scripting;

public class UIVoucherElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIVoucherElement> { }

	[SerializeField] UIVoucherTitle       m_Title;
	[SerializeField] UIVoucherDescription m_Description;
	[SerializeField] UIVoucherButton      m_Button;

	public void Setup(string _VoucherID)
	{
		m_Title.VoucherID       = _VoucherID;
		m_Description.VoucherID = _VoucherID;
		m_Button.VoucherID      = _VoucherID;
	}
}
