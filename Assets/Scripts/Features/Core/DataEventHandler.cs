using System;
using System.Collections.Generic;

public class DataEventHandler<T>
{
	readonly Dictionary<T, Action>    m_RegularListeners = new Dictionary<T, Action>();
	readonly Dictionary<T, Action<T>> m_DynamicListeners = new Dictionary<T, Action<T>>();

	Action    m_RegularAction;
	Action<T> m_DynamicAction;

	public void AddListener(Action _Action) => m_RegularAction += _Action;

	public void RemoveListener(Action _Action) => m_RegularAction -= _Action;

	public void AddListener(Action<T> _Action) => m_DynamicAction += _Action;

	public void RemoveListener(Action<T> _Action) => m_DynamicAction -= _Action;
	
	public void AddListener(T _ID, Action _Action)
	{
		if (_ID == null)
			return;
		
		if (m_RegularListeners.ContainsKey(_ID))
			m_RegularListeners[_ID] += _Action;
		else
			m_RegularListeners[_ID] = _Action;
	}

	public void AddListener(T _ID, Action<T> _Action)
	{
		if (_ID == null)
			return;
		
		if (m_DynamicListeners.ContainsKey(_ID))
			m_DynamicListeners[_ID] += _Action;
		else
			m_DynamicListeners[_ID] = _Action;
	}

	public void RemoveListener(T _ID, Action _Action)
	{
		if (_ID == null)
			return;
		
		if (m_RegularListeners.ContainsKey(_ID))
			m_RegularListeners[_ID] -= _Action;
	}

	public void RemoveListener(T _ID, Action<T> _Action)
	{
		if (_ID == null)
			return;
		
		if (m_DynamicListeners.ContainsKey(_ID))
			m_DynamicListeners[_ID] -= _Action;
	}

	public void Invoke(T _ID)
	{
		if (_ID == null)
			return;
		
		if (m_RegularListeners.TryGetValue(_ID, out Action regular))
			regular?.Invoke();
		
		if (m_DynamicListeners.TryGetValue(_ID, out Action<T> dynamic))
			dynamic?.Invoke(_ID);
		
		m_RegularAction?.Invoke();
		
		m_DynamicAction?.Invoke(_ID);
	}
}

public class DataEventHandler : DataEventHandler<string>
{
	public DataEventType Type { get; }

	public DataEventHandler()
	{
		Type = DataEventType.None;
	}

	public DataEventHandler(DataEventType _Type)
	{
		Type = _Type;
	}
}
