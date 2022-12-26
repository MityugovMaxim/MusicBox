using System;
using System.Globalization;

public class AdminNumberNode : AdminNode
{
	public override AdminNodeType Type => AdminNodeType.Number;

	public decimal Value
	{
		get => Convert.ToDecimal(Data.GetValue(Path));
		set
		{
			if (Data.Contains(Path) && Value == value)
				return;
			
			Data.SetValue(Path, value);
			
			Changed = Value != m_Default;
			
			InvokeValueChanged();
		}
	}

	decimal m_Default;

	public AdminNumberNode(AdminData _Data, AdminNode _Node, string _Path) : base(_Data, _Node, _Path) { }

	public override void Create()
	{
		Changed = false;
		
		if (Data.Contains(Path))
			m_Default = Value;
		else
			Value = m_Default = 0;
	}

	public override void Restore() => Value = m_Default;

	public override string Copy() => Value.ToString(CultureInfo.InvariantCulture);

	public override void Paste(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && decimal.TryParse(_Value, out decimal value))
			Value = value;
	}
}
