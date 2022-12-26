using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class UIAdminTickNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	[SerializeField] UIAdminField   m_ValueField;
	[SerializeField] UIRepeatButton m_IncrementButton;
	[SerializeField] UIRepeatButton m_DecrementButton;

	AdminNumberNode m_Node;
	decimal[]       m_Intervals;
	int             m_Count;
	decimal         m_Min = decimal.MinValue;
	decimal         m_Max = decimal.MaxValue;

	protected override void Awake()
	{
		base.Awake();
		
		m_ValueField.Subscribe(ProcessValue);
		m_IncrementButton.Subscribe(Increment);
		m_IncrementButton.SubscribeStart(StartRepeat);
		m_IncrementButton.SubscribeStop(StopRepeat);
		m_DecrementButton.Subscribe(Decrement);
		m_DecrementButton.SubscribeStart(StartRepeat);
		m_DecrementButton.SubscribeStop(StopRepeat);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ValueField.Unsubscribe(ProcessValue);
		m_IncrementButton.Unsubscribe(Increment);
		m_IncrementButton.UnsubscribeStart(StartRepeat);
		m_IncrementButton.UnsubscribeStop(StopRepeat);
		m_DecrementButton.Unsubscribe(Decrement);
		m_DecrementButton.UnsubscribeStart(StartRepeat);
		m_DecrementButton.UnsubscribeStop(StopRepeat);
	}

	public override void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		base.Setup(_Parent, _Node);
		
		m_Node = Node as AdminNumberNode;
		
		if (Node.TryGetAttribute(out AdminTickAttribute tick))
			m_Intervals = tick.Intervals;
		
		if (m_Intervals == null || m_Intervals.Length == 0)
			m_Intervals = new decimal[] { 1 };
		
		if (Node.TryGetAttribute(out AdminLimitAttribute limit))
		{
			m_Min = limit.Min;
			m_Max = limit.Max;
		}
		
		ValueChanged();
	}

	decimal GetValue()
	{
		return m_Intervals.Aggregate(
			(_A, _B) =>
			{
				if (_A <= m_Count && _B <= m_Count)
					return _A > _B ? _A : _B;
				
				if (_A > m_Count && _B > m_Count)
					return _A < _B ? _A : _B;
				
				if (_A <= m_Count && _B > m_Count)
					return _A;
				
				if (_B <= m_Count && _A > m_Count)
					return _B;
				
				return _A > _B ? _A : _B;
			}
		);
	}

	void ProcessValue(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && decimal.TryParse(_Value, out decimal value))
			m_Node.Value = value;
	}

	void Increment()
	{
		if (m_Node == null)
			return;
		
		decimal offset = GetValue();
		
		decimal value = Math.Clamp(m_Node.Value + offset, m_Min, m_Max);
		
		m_Node.Value = value;
		
		m_Count += (int)offset;
	}

	void Decrement()
	{
		if (m_Node == null)
			return;
		
		decimal offset = GetValue();
		
		decimal value = Math.Clamp(m_Node.Value - offset, m_Min, m_Max);
		
		m_Node.Value = value;
		
		m_Count += (int)offset;
	}

	void StartRepeat()
	{
		m_Count = 0;
	}

	void StopRepeat()
	{
		m_Count = 0;
	}

	protected override void ValueChanged()
	{
		if (m_Node == null)
			return;
		
		m_ValueField.Value = m_Node.Value.ToString(CultureInfo.InvariantCulture);
	}
}
