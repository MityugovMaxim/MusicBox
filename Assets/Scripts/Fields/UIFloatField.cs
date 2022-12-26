using System.Globalization;

public class UIFloatField : UIInputField<float>
{
	protected override string ParseString(float _Value) => _Value.ToString(CultureInfo.InvariantCulture);

	protected override float ParseValue(string _Text) => !string.IsNullOrEmpty(_Text) && float.TryParse(_Text, out float value) ? value : Origin;
}
