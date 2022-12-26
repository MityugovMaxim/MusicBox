using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

public class UIDataArea : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataArea> { }

	[SerializeField] TMP_InputField m_Field;
	[SerializeField] UIButton       m_EditButton;

	protected override void Awake()
	{
		base.Awake();
		
		m_Field.enabled = false;
		m_Field.onEndEdit.AddListener(ProcessField);
		
		m_EditButton.Subscribe(Edit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Field.onEndEdit.RemoveListener(ProcessField);
		
		m_EditButton.Unsubscribe(Edit);
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
		else if (DataNode.Type == typeof(string))
		{
			m_Field.contentType  = TMP_InputField.ContentType.Standard;
			m_Field.keyboardType = TouchScreenKeyboardType.Default;
		}
	}

	protected override void ProcessValue()
	{
		base.ProcessValue();
		
		m_Field.SetTextWithoutNotify(Value);
	}
}
