using System.Globalization;

public class UIDoubleField : UIInputField<double>
{
	protected override string ParseString(double _Value) => _Value.ToString(CultureInfo.InvariantCulture);

	protected override double ParseValue(string _Text) => !string.IsNullOrEmpty(_Text) && double.TryParse(_Text, out double value) ? value : Origin;
}