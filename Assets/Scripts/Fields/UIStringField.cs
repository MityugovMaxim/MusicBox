using System.Reflection;

public class UIStringField : UIInputField<string>
{
	public void Setup(object _Target, string _Property)
	{
		if (_Target == null)
			return;
		
		PropertyInfo propertyInfo = _Target.GetType().GetProperty(_Property);
		
		Setup(_Target, propertyInfo);
	}

	protected override string ParseString(string _Value) => _Value;

	protected override string ParseValue(string _Text) => _Text;
}