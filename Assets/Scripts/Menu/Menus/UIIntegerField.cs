using TMPro;
using UnityEngine;

public class UIIntegerField : UIField<int>
{
	protected override TMP_InputField.ContentType ContentType => TMP_InputField.ContentType.IntegerNumber;

	protected override TouchScreenKeyboardType KeyboardType => TouchScreenKeyboardType.NumberPad;

	protected override int Parse(string _Text)
	{
		return int.TryParse(_Text, out int value) ? value : 0;
	}
}