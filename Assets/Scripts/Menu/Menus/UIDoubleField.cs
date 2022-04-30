using TMPro;
using UnityEngine;

public class UIDoubleField : UIField<double>
{
	protected override TMP_InputField.ContentType ContentType => TMP_InputField.ContentType.DecimalNumber;

	protected override TouchScreenKeyboardType KeyboardType => TouchScreenKeyboardType.DecimalPad;

	protected override double Parse(string _Text)
	{
		return double.TryParse(_Text, out double value) ? value : 0;
	}
}