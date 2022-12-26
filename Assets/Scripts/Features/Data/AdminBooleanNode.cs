public class AdminBooleanNode : AdminNode
{
	public override AdminNodeType Type => AdminNodeType.Boolean;

	public bool Value
	{
		get => Data.GetValue<bool>(Path);
		set
		{
			if (Data.Contains(Path) && Value == value)
				return;
			
			Data.SetValue(Path, value);
			
			Changed = Value != m_Default;
			
			InvokeValueChanged();
		}
	}

	bool m_Default;

	public AdminBooleanNode(AdminData _Data, AdminNode _Node, string _Path) : base(_Data, _Node, _Path) { }

	public override void Create()
	{
		Changed = false;
		
		if (Data.Contains(Path))
			m_Default = Value;
		else
			Value = m_Default = false;
	}

	public override void Restore() => Value = m_Default;

	public override string Copy() => Value.ToString();

	public override void Paste(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && bool.TryParse(_Value, out bool value))
			Value = value;
	}
}
