using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIDataEnum : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataEnum> { }

	[SerializeField] TMP_Text m_Value;
	[SerializeField] Button   m_NextButton;
	[SerializeField] Button   m_PreviousButton;

	protected override void Awake()
	{
		base.Awake();
		
		m_NextButton.Subscribe(Next);
		m_PreviousButton.Subscribe(Previous);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_NextButton.Unsubscribe(Next);
		m_PreviousButton.Unsubscribe(Previous);
	}

	protected override void ProcessValue()
	{
		base.ProcessValue();
		
		m_Value.text = Value;
	}

	void Next()
	{
		Enum value = GetValue<Enum>();
		
		int index = GetValueIndex(value);
		
		if (index < 0)
			return;
		
		SetValueIndex(index + 1);
	}

	void Previous()
	{
		Enum value = GetValue<Enum>();
		
		int index = GetValueIndex(value);
		
		if (index < 0)
			return;
		
		SetValueIndex(index - 1);
	}

	void SetValueIndex(int _Index)
	{
		Array values = Enum.GetValues(DataNode.Type);
		
		if (_Index < 0 || _Index >= values.Length)
			return;
		
		_Index = MathUtility.Repeat(_Index, values.Length);
		
		SetValue(values.GetValue(_Index));
	}

	int GetValueIndex(Enum _Value)
	{
		Array array = Enum.GetValues(DataNode.Type);
		
		for (int i = 0; i < array.Length; i++)
		{
			if (Equals(_Value, array.GetValue(i)))
				return i;
		}
		
		return -1;
	}
}
