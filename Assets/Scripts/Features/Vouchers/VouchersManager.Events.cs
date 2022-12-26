using System;

public partial class VouchersManager
{
	public void SubscribeStart(Action _Action) => m_StartHandler.AddListener(_Action);

	public void SubscribeStart(Action<string> _Action) => m_StartHandler.AddListener(_Action);

	public void SubscribeStart(string _VoucherID, Action _Action) => m_StartHandler.AddListener(_VoucherID, _Action);

	public void SubscribeStart(string _VoucherID, Action<string> _Action) => m_StartHandler.AddListener(_VoucherID, _Action);

	public void SubscribeEnd(Action _Action) => m_EndHandler.AddListener(_Action);

	public void SubscribeEnd(Action<string> _Action) => m_EndHandler.AddListener(_Action);

	public void SubscribeEnd(string _VoucherID, Action _Action) => m_EndHandler.AddListener(_VoucherID, _Action);

	public void SubscribeEnd(string _VoucherID, Action<string> _Action) => m_EndHandler.AddListener(_VoucherID, _Action);

	public void SubscribeCancel(Action _Action) => m_CancelHandler.AddListener(_Action);

	public void SubscribeCancel(Action<string> _Action) => m_CancelHandler.AddListener(_Action);

	public void SubscribeCancel(string _VoucherID, Action _Action) => m_CancelHandler.AddListener(_VoucherID, _Action);

	public void SubscribeCancel(string _VoucherID, Action<string> _Action) => m_CancelHandler.AddListener(_VoucherID, _Action);

	public void UnsubscribeStart(Action _Action) => m_StartHandler.RemoveListener(_Action);

	public void UnsubscribeStart(Action<string> _Action) => m_StartHandler.RemoveListener(_Action);

	public void UnsubscribeStart(string _VoucherID, Action _Action) => m_StartHandler.RemoveListener(_VoucherID, _Action);

	public void UnsubscribeStart(string _VoucherID, Action<string> _Action) => m_StartHandler.RemoveListener(_VoucherID, _Action);

	public void UnsubscribeEnd(Action _Action) => m_EndHandler.RemoveListener(_Action);

	public void UnsubscribeEnd(Action<string> _Action) => m_EndHandler.RemoveListener(_Action);

	public void UnsubscribeEnd(string _VoucherID, Action _Action) => m_EndHandler.RemoveListener(_VoucherID, _Action);

	public void UnsubscribeEnd(string _VoucherID, Action<string> _Action) => m_EndHandler.RemoveListener(_VoucherID, _Action);

	public void UnsubscribeCancel(Action _Action) => m_CancelHandler.RemoveListener(_Action);

	public void UnsubscribeCancel(Action<string> _Action) => m_CancelHandler.RemoveListener(_Action);

	public void UnsubscribeCancel(string _VoucherID, Action _Action) => m_CancelHandler.RemoveListener(_VoucherID, _Action);

	public void UnsubscribeCancel(string _VoucherID, Action<string> _Action) => m_CancelHandler.RemoveListener(_VoucherID, _Action);
}