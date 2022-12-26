using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIVoucherElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIVoucherElement> { }

	[SerializeField] UIVoucherTitle       m_Title;
	[SerializeField] UIVoucherDescription m_Description;
	[SerializeField] UIVoucherTimer       m_Timer;
	[SerializeField] UIVoucherAction      m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _VoucherID)
	{
		m_Title.VoucherID       = _VoucherID;
		m_Description.VoucherID = _VoucherID;
		m_Timer.VoucherID       = _VoucherID;
		m_Action.VoucherID      = _VoucherID;
		
		m_BadgeManager.ReadVoucher(_VoucherID);
	}
}
