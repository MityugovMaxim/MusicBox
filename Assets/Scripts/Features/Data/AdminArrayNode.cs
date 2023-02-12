using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing;

public class AdminArrayNode : AdminNode
{
	public override AdminNodeType Type => AdminNodeType.Array;

	public IList<object> Value
	{
		get => Data.GetValue<IList<object>>(Path);
		set
		{
			if (Data.Contains(Path) && Equals(Value, value))
				return;
			
			Data.SetValue(Path, value);
			
			Changed = CheckChanged();
			
			InvokeValueChanged();
		}
	}

	readonly HashSet<string> m_Default =new HashSet<string>();

	public AdminArrayNode(AdminData _Data, AdminNode _Node, string _Path) : base(_Data, _Node, _Path) { }

	public override void Create()
	{
		Changed = false;
		
		m_Default.Clear();
		foreach (AdminNode node in Children)
			m_Default.Add(node.Name);
		
		if (!Data.Contains(Path))
			Value = new List<object>();
		
		foreach (AdminNode node in Children)
			node.Create();
		
		InvokeValueChanged();
	}

	public override void Restore() { }

	public override string Copy() => MiniJson.JsonEncode(Value);

	public override void Paste(string _Value)
	{
		IList<object> data = MiniJson.JsonDecode(_Value) as IList<object>;
		
		if (data == null)
			return;
		
		// Remove
		foreach (AdminNode node in Children.ToArray())
		{
			if (node.HasAttribute<AdminFixedAttribute>())
				continue;
			
			node.Remove();
		}
		
		// Add
		for (int i = 0; i < data.Count; i++)
		{
			string path  = $"{Path}/[{i}]";
			
			if (Data.Contains(path))
				continue;
			
			AdminNode node = Data.CreateObject(this, path);
			
			if (node == null)
				continue;
			
			node.Create();
		}
		
		// Update
		foreach (AdminNode node in Children)
		{
			if (node == null || string.IsNullOrEmpty(node.Name) || !AdminUtility.TryGetIndex(node.Name, out int index))
				continue;
			
			object value = data[index];
			
			if (node.Type == AdminNodeType.Object || node.Type == AdminNodeType.Array)
				node.Paste(MiniJson.JsonEncode(value));
			else
				node.Paste(value.ToString());
		}
	}

	protected override void AddNode(AdminNode _Node)
	{
		base.AddNode(_Node);
		
		Changed = CheckChanged();
		
		InvokeValueChanged();
	}

	protected override void RemoveNode(AdminNode _Node)
	{
		base.RemoveNode(_Node);
		
		Changed = CheckChanged();
		
		InvokeValueChanged();
	}

	bool CheckChanged()
	{
		if (m_Default.Count != Children.Count)
			return true;
		
		foreach (AdminNode node in Children)
		{
			if (m_Default.Contains(node.Name))
				continue;
			
			return true;
		}
		
		return false;
	}
}
