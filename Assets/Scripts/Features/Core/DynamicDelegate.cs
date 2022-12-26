using System;

public class DynamicDelegate
{
	Action m_RegularAction;
	
	public static DynamicDelegate operator +(DynamicDelegate _Delegate, Action _Action)
	{
		if (_Delegate == null)
			_Delegate = new DynamicDelegate();
		
		_Delegate.AddListener(_Action);
		
		return _Delegate;
	}

	public static DynamicDelegate operator -(DynamicDelegate _Delegate, Action _Action)
	{
		if (_Delegate == null)
			return null;
		
		_Delegate.RemoveListener(_Action);
		
		return _Delegate;
	}

	public void AddListener(Action _Action)
	{
		m_RegularAction += _Action;
	}

	public void RemoveListener(Action _Action)
	{
		m_RegularAction -= _Action;
	}

	public void Invoke()
	{
		m_RegularAction?.Invoke();
	}
}

public class DynamicDelegate<T0, T1>
{
	Action         m_RegularAction;
	Action<T0, T1> m_DynamicAction;

	public static DynamicDelegate<T0, T1> operator +(DynamicDelegate<T0, T1> _Delegate, Action _Action)
	{
		if (_Delegate == null)
			_Delegate = new DynamicDelegate<T0, T1>();
		
		_Delegate.AddListener(_Action);
		
		return _Delegate;
	}

	public static DynamicDelegate<T0, T1> operator +(DynamicDelegate<T0, T1> _Delegate, Action<T0, T1> _Action)
	{
		if (_Delegate == null)
			_Delegate = new DynamicDelegate<T0, T1>();
		
		_Delegate.AddListener(_Action);
		
		return _Delegate;
	}

	public static DynamicDelegate<T0, T1> operator -(DynamicDelegate<T0, T1> _Delegate, Action _Action)
	{
		if (_Delegate == null)
			return null;
		
		_Delegate.RemoveListener(_Action);
		
		return _Delegate;
	}

	public static DynamicDelegate<T0, T1> operator -(DynamicDelegate<T0, T1> _Delegate, Action<T0, T1> _Action)
	{
		if (_Delegate == null)
			return null;
		
		_Delegate.RemoveListener(_Action);
		
		return _Delegate;
	}

	public void AddListener(Action _Action)
	{
		m_RegularAction += _Action;
	}

	public void AddListener(Action<T0, T1> _Action)
	{
		m_DynamicAction += _Action;
	}

	public void RemoveListener(Action _Action)
	{
		m_RegularAction -= _Action;
	}

	public void RemoveListener(Action<T0, T1> _Action)
	{
		m_DynamicAction -= _Action;
	}

	public void Invoke(T0 _Arg0, T1 _Arg1)
	{
		m_RegularAction?.Invoke();
		
		m_DynamicAction?.Invoke(_Arg0, _Arg1);
	}
}

public class DynamicDelegate<T>
{
	Action    m_RegularAction;
	Action<T> m_DynamicAction;

	public static DynamicDelegate<T> operator +(DynamicDelegate<T> _Delegate, Action _Action)
	{
		if (_Delegate == null)
			_Delegate = new DynamicDelegate<T>();
		
		_Delegate.AddListener(_Action);
		
		return _Delegate;
	}

	public static DynamicDelegate<T> operator +(DynamicDelegate<T> _Delegate, Action<T> _Action)
	{
		if (_Delegate == null)
			_Delegate = new DynamicDelegate<T>();
		
		_Delegate.AddListener(_Action);
		
		return _Delegate;
	}

	public static DynamicDelegate<T> operator -(DynamicDelegate<T> _Delegate, Action _Action)
	{
		if (_Delegate == null)
			return null;
		
		_Delegate.RemoveListener(_Action);
		
		return _Delegate;
	}

	public static DynamicDelegate<T> operator -(DynamicDelegate<T> _Delegate, Action<T> _Action)
	{
		if (_Delegate == null)
			return null;
		
		_Delegate.RemoveListener(_Action);
		
		return _Delegate;
	}

	public void AddListener(Action _Action)
	{
		m_RegularAction += _Action;
	}

	public void AddListener(Action<T> _Action)
	{
		m_DynamicAction += _Action;
	}

	public void RemoveListener(Action _Action)
	{
		m_RegularAction -= _Action;
	}

	public void RemoveListener(Action<T> _Action)
	{
		m_DynamicAction -= _Action;
	}

	public void Invoke(T _Value)
	{
		m_RegularAction?.Invoke();
		
		m_DynamicAction?.Invoke(_Value);
	}
}
