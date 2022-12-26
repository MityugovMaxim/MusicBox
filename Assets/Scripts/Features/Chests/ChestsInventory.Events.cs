using System;

public partial class ChestsInventory
{
	public void SubscribeStart(Action _Action) => m_StartHandler.AddListener(_Action);

	public void SubscribeStart(Action<string> _Action) => m_StartHandler.AddListener(_Action);

	public void SubscribeStart(string _ChestID, Action _Action) => m_StartHandler.AddListener(_ChestID, _Action);

	public void SubscribeStart(string _ChestID, Action<string> _Action) => m_StartHandler.AddListener(_ChestID, _Action);

	public void SubscribeEnd(Action _Action) => m_EndHandler.AddListener(_Action);

	public void SubscribeEnd(Action<string> _Action) => m_EndHandler.AddListener(_Action);

	public void SubscribeEnd(string _ChestID, Action _Action) => m_EndHandler.AddListener(_ChestID, _Action);

	public void SubscribeEnd(string _ChestID, Action<string> _Action) => m_EndHandler.AddListener(_ChestID, _Action);

	public void SubscribeCancel(Action _Action) => m_CancelHandler.AddListener(_Action);

	public void SubscribeCancel(Action<string> _Action) => m_CancelHandler.AddListener(_Action);

	public void SubscribeCancel(string _ChestID, Action _Action) => m_CancelHandler.AddListener(_ChestID, _Action);

	public void SubscribeCancel(string _ChestID, Action<string> _Action) => m_CancelHandler.AddListener(_ChestID, _Action);

	public void UnsubscribeStart(Action _Action) => m_StartHandler.RemoveListener(_Action);

	public void UnsubscribeStart(Action<string> _Action) => m_StartHandler.RemoveListener(_Action);

	public void UnsubscribeStart(string _ChestID, Action _Action) => m_StartHandler.RemoveListener(_ChestID, _Action);

	public void UnsubscribeStart(string _ChestID, Action<string> _Action) => m_StartHandler.RemoveListener(_ChestID, _Action);

	public void UnsubscribeEnd(Action _Action) => m_EndHandler.RemoveListener(_Action);

	public void UnsubscribeEnd(Action<string> _Action) => m_EndHandler.RemoveListener(_Action);

	public void UnsubscribeEnd(string _ChestID, Action _Action) => m_EndHandler.RemoveListener(_ChestID, _Action);

	public void UnsubscribeEnd(string _ChestID, Action<string> _Action) => m_EndHandler.RemoveListener(_ChestID, _Action);

	public void UnsubscribeCancel(Action _Action) => m_CancelHandler.RemoveListener(_Action);

	public void UnsubscribeCancel(Action<string> _Action) => m_CancelHandler.RemoveListener(_Action);

	public void UnsubscribeCancel(string _ChestID, Action _Action) => m_CancelHandler.RemoveListener(_ChestID, _Action);

	public void UnsubscribeCancel(string _ChestID, Action<string> _Action) => m_CancelHandler.RemoveListener(_ChestID, _Action);
}
