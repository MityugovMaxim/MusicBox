public class UIStringField : UIInputField<string>
{
	protected override string ParseString(string _Value) => _Value;

	protected override string ParseValue(string _Text) => _Text;
}