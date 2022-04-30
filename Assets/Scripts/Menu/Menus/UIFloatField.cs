using TMPro;
using UnityEngine;

public class UIFloatField : UIField<float>
{
	protected override TMP_InputField.ContentType ContentType => TMP_InputField.ContentType.DecimalNumber;

	protected override TouchScreenKeyboardType KeyboardType => TouchScreenKeyboardType.DecimalPad;

	protected override float Parse(string _Text)
	{
		return float.TryParse(_Text, out float value) ? value : 0;
	}
}