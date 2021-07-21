using System;

public abstract class ActionLink
{
	protected ActionChain ActionChain => m_ActionChain;

	ActionChain m_ActionChain;
	Action      m_Finished;

	public void Execute(ActionChain _ActionChain, Action _Finished = null)
	{
		m_ActionChain = _ActionChain;
		m_Finished    = _Finished;
	}

	protected void Complete()
	{
		m_Finished?.Invoke();
	}

	protected abstract void Process();
}

public abstract class ActionLink<TData> : ActionLink
{
	protected TData Data => m_Data;

	readonly TData m_Data;

	public ActionLink(TData _Data)
	{
		m_Data = _Data;
	}
}