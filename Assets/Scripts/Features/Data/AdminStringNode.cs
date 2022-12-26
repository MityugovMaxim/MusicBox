using UnityEngine;

public class AdminStringNode : AdminNode
{
	public override AdminNodeType Type => AdminNodeType.String;

	public string Value
	{
		get => Data.GetValue<string>(Path);
		set
		{
			if (Data.Contains(Path) && Value == value)
				return;
			
			Data.SetValue(Path, value);
			
			Changed = Value != m_Default;
			
			InvokeValueChanged();
		}
	}

	string m_Default;

	public AdminStringNode(AdminData _Data, AdminNode _Node, string _Path) : base(_Data, _Node, _Path) { }

	public override void Create()
	{
		Changed = false;
		
		if (Data.Contains(Path))
			m_Default = Value;
		else
			Value = m_Default = string.Empty;
	}

	public override void Restore() => Value = m_Default;

	public override string Copy() => Value;

	public override void Paste(string _Value) => Value = _Value ?? string.Empty;
}
