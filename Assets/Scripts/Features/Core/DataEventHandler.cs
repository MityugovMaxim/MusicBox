using System;
using System.Collections.Generic;

public class DataEventHandler
{
	public DataEventType Type { get; }

	readonly Dictionary<string, Action>         m_RegularListeners = new Dictionary<string, Action>();
	readonly Dictionary<string, Action<string>> m_DynamicListeners = new Dictionary<string, Action<string>>();

	Action         m_RegularAction;
	Action<string> m_DynamicAction;

	public DataEventHandler()
	{
		Type = DataEventType.None;
	}

	public DataEventHandler(DataEventType _Type)
	{
		Type = _Type;
	}

	public void AddListener(Action _Action)
	{
		m_RegularAction += _Action;
	}

	public void AddListener(Action<string> _Action)
	{
		m_DynamicAction += _Action;
	}

	public void AddListener(string _ID, Action _Action)
	{
		if (_ID == null)
			return;
		
		if (m_RegularListeners.ContainsKey(_ID))
			m_RegularListeners[_ID] += _Action;
		else
			m_RegularListeners[_ID] = _Action;
	}

	public void AddListener(string _ID, Action<string> _Action)
	{
		if (_ID == null)
			return;
		
		if (m_DynamicListeners.ContainsKey(_ID))
			m_DynamicListeners[_ID] += _Action;
		else
			m_DynamicListeners[_ID] = _Action;
	}

	public void RemoveListener(Action _Action)
	{
		m_RegularAction -= _Action;
	}

	public void RemoveListener(Action<string> _Action)
	{
		m_DynamicAction -= _Action;
	}

	public void RemoveListener(string _ID, Action _Action)
	{
		if (_ID == null)
			return;
		
		if (m_RegularListeners.ContainsKey(_ID))
			m_RegularListeners[_ID] -= _Action;
	}

	public void RemoveListener(string _ID, Action<string> _Action)
	{
		if (_ID == null)
			return;
		
		if (m_DynamicListeners.ContainsKey(_ID))
			m_DynamicListeners[_ID] -= _Action;
	}

	public void Invoke(string _ID)
	{
		if (_ID == null)
			return;
		
		if (m_RegularListeners.TryGetValue(_ID, out Action regular))
			regular?.Invoke();
		
		if (m_DynamicListeners.TryGetValue(_ID, out Action<string> dynamic))
			dynamic?.Invoke(_ID);
		
		m_RegularAction?.Invoke();
		
		m_DynamicAction?.Invoke(_ID);
	}
}
