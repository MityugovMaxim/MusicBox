public class UILongField : UIInputField<long>
{
	protected override string ParseString(long _Value) => _Value.ToString();

	protected override long ParseValue(string _Text) => !string.IsNullOrEmpty(_Text) && long.TryParse(_Text, out long value) ? value : Origin;
}