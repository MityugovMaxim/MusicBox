public class UIIntegerField : UIInputField<int>
{
	protected override string ParseString(int _Value) => _Value.ToString();

	protected override int ParseValue(string _Text) => !string.IsNullOrEmpty(_Text) && int.TryParse(_Text, out int value) ? value : Origin;
}