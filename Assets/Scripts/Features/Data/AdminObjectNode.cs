using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing;

public class AdminObjectNode : AdminNode
{
	public override AdminNodeType Type => AdminNodeType.Object;

	public IDictionary<string, object> Value
	{
		get => Data.GetValue<IDictionary<string, object>>(Path);
		set
		{
			if (Data.Contains(Path) && Value == value)
				return;
			
			Data.SetValue(Path, value);
			
			Changed = CheckChanged();
			
			InvokeValueChanged();
		}
	}

	readonly HashSet<string> m_Default = new HashSet<string>();

	public AdminObjectNode(AdminData _Data, AdminNode _Node, string _Path) : base(_Data, _Node, _Path) { }

	public override void Create()
	{
		Changed = false;
		
		m_Default.Clear();
		foreach (AdminNode node in Children)
			m_Default.Add(node.Name);
		
		if (!Data.Contains(Path))
			Value = new Dictionary<string, object>();
		
		foreach (AdminNode node in Children)
			node.Create();
		
		InvokeValueChanged();
	}

	public override void Restore() { }

	public override string Copy() => MiniJson.JsonEncode(Value);

	public override void Paste(string _Value)
	{
		IDictionary<string, object> data = MiniJson.JsonDecode(_Value) as IDictionary<string, object>;
		
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
		foreach (var entry in data)
		{
			string path = $"{Path}/{entry.Key}";
			
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
			if (node == null || string.IsNullOrEmpty(node.Name) || !data.ContainsKey(node.Name))
				continue;
			
			object value = data[node.Name];
			
			if (node.Type == AdminNodeType.Object || node.Type == AdminNodeType.Array)
				node.Paste(MiniJson.JsonEncode(value));
			else
				node.Paste(value.ToString());
		}
		
		InvokeValueChanged();
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
