using TMPro;
using UnityEngine;

public class UILocalizationField : UIEntity
{
	public string Value
	{
		get => m_Field.text;
		private set => m_Field.text = value;
	}

	public TMP_InputField.SubmitEvent Submit => m_Field.onEndEdit;

	[SerializeField] TMP_InputField m_Field;

	public void SetValue(string _Value)
	{
		m_Field.SetTextWithoutNotify(_Value);
	}

	public void Copy()
	{
		GUIUtility.systemCopyBuffer = Value;
	}

	public void Paste()
	{
		Value = GUIUtility.systemCopyBuffer;
		
		Submit.Invoke(Value);
	}

	public void Upper()
	{
		Value = Value.ToUpperInvariant();
		
		Submit.Invoke(Value);
	}

	public void Lower()
	{
		Value = Value.ToLowerInvariant();
		
		Submit.Invoke(Value);
	}

	public void Capitalize()
	{
		Value = Value.ToDisplayName();
		
		Submit.Invoke(Value);
	}

	public void Caps()
	{
		Value = Value.ToAllCapital();
		
		Submit.Invoke(Value);
	}
}