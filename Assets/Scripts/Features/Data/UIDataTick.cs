using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIDataTick : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataTick> { }

	[SerializeField] TMP_InputField m_Field;
	[SerializeField] UIButton       m_EditButton;
	[SerializeField] Button         m_IncrementButton;
	[SerializeField] Button         m_DecrementButton;

	protected override void Awake()
	{
		base.Awake();
		
		m_Field.enabled = false;
		
		m_Field.Subscribe(ProcessField);
		m_EditButton.Subscribe(Edit);
		m_IncrementButton.Subscribe(Increment);
		m_DecrementButton.Subscribe(Decrement);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Field.Unsubscribe(ProcessField);
		m_EditButton.Unsubscribe(Edit);
		m_IncrementButton.Unsubscribe(Increment);
		m_DecrementButton.Unsubscribe(Decrement);
	}

	protected override void ProcessData()
	{
		base.ProcessData();
		
		if (DataNode.Type == typeof(int))
		{
			m_Field.contentType  = TMP_InputField.ContentType.IntegerNumber;
			m_Field.keyboardType = TouchScreenKeyboardType.NumberPad;
		}
		else if (DataNode.Type == typeof(long))
		{
			m_Field.contentType  = TMP_InputField.ContentType.IntegerNumber;
			m_Field.keyboardType = TouchScreenKeyboardType.NumberPad;
		}
		else if (DataNode.Type == typeof(float))
		{
			m_Field.contentType  = TMP_InputField.ContentType.DecimalNumber;
			m_Field.keyboardType = TouchScreenKeyboardType.DecimalPad;
		}
		else if (DataNode.Type == typeof(double))
		{
			m_Field.contentType  = TMP_InputField.ContentType.DecimalNumber;
			m_Field.keyboardType = TouchScreenKeyboardType.DecimalPad;
		}
		else if (DataNode.Type == typeof(decimal))
		{
			m_Field.contentType  = TMP_InputField.ContentType.DecimalNumber;
			m_Field.keyboardType = TouchScreenKeyboardType.DecimalPad;
		}
	}

	protected override void ProcessValue()
	{
		base.ProcessValue();
		
		m_Field.SetTextWithoutNotify(Value);
	}

	void Increment()
	{
		m_Field.enabled = false;
		m_Field.DeactivateInputField();
		
		if (DataNode.Type == typeof(int))
			SetValue(DataNode.GetValue<int>() + 1);
		else if (DataNode.Type == typeof(long))
			SetValue(DataNode.GetValue<long>() + 1);
		else if (DataNode.Type == typeof(float))
			SetValue(DataNode.GetValue<float>() + 1);
		else if (DataNode.Type == typeof(double))
			SetValue(DataNode.GetValue<double>() + 1);
		else if (DataNode.Type == typeof(decimal))
			SetValue(DataNode.GetValue<decimal>() + 1);
	}

	void Decrement()
	{
		m_Field.enabled = false;
		m_Field.DeactivateInputField();
		
		if (DataNode.Type == typeof(int))
			SetValue(DataNode.GetValue<int>() - 1);
		else if (DataNode.Type == typeof(long))
			SetValue(DataNode.GetValue<long>() - 1);
		else if (DataNode.Type == typeof(float))
			SetValue(DataNode.GetValue<float>() - 1);
		else if (DataNode.Type == typeof(double))
			SetValue(DataNode.GetValue<double>() - 1);
		else if (DataNode.Type == typeof(decimal))
			SetValue(DataNode.GetValue<decimal>() - 1);
	}

	void Edit()
	{
		m_Field.enabled = true;
		m_Field.ActivateInputField();
	}

	void ProcessField(string _Text)
	{
		m_Field.enabled = false;
		m_Field.DeactivateInputField();
		
		Value = _Text;
	}
}
