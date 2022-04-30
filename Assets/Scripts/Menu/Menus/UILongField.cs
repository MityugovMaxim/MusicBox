using TMPro;
using UnityEngine;

public class UILongField : UIField<long>
{
	protected override TMP_InputField.ContentType ContentType => TMP_InputField.ContentType.IntegerNumber;

	protected override TouchScreenKeyboardType KeyboardType => TouchScreenKeyboardType.NumberPad;

	protected override long Parse(string _Text)
	{
		return long.TryParse(_Text, out long value) ? value : 0;
	}
}