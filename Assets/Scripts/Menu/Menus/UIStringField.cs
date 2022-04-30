using TMPro;
using UnityEngine;

public class UIStringField : UIField<string>
{
	protected override TMP_InputField.ContentType ContentType => TMP_InputField.ContentType.Standard;

	protected override TouchScreenKeyboardType KeyboardType => TouchScreenKeyboardType.Default;

	protected override string Parse(string _Text)
	{
		return _Text ?? string.Empty;
	}
}