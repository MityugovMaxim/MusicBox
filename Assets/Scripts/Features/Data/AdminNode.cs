using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AdminNode
{
	public abstract AdminNodeType Type { get; }

	public AdminData                Data     { get; }
	public string                   Path     { get; private set; }
	public string                   Name     { get; private set; }
	public AdminNode                Parent   { get; private set; }
	public int                      Indent   { get; private set; }
	public bool                     Expanded { get; set; }
	public IReadOnlyList<AdminNode> Children => m_Children;

	public bool Changed
	{
		get
		{
			if (m_Changed)
				return true;
			
			foreach (AdminNode node in Children)
			{
				if (node.Changed)
					return true;
			}
			
			return false;
		}
		protected set => m_Changed = value;
	}

	readonly List<AdminNode>  m_Children;
	readonly DataEventHandler m_ValueChanged;

	bool m_Changed;

	protected AdminNode(AdminData _Data, AdminNode _Node, string _Path)
	{
		Data = _Data;
		Path = _Path;
		Name = AdminUtility.GetName(Path);
		
		Parent         = _Node;
		m_Children     = new List<AdminNode>();
		m_ValueChanged = new DataEventHandler();
		
		Parent?.AddNode(this);
	}

	public void Subscribe(Action _Action) => m_ValueChanged.AddListener(Path, _Action);

	public void Unsubscribe(Action _Action) => m_ValueChanged.RemoveListener(Path, _Action);

	public abstract void Create();

	public abstract void Restore();

	public void Insert()
	{
		if (!TryGetAttribute(out AdminCollectionAttribute attribute))
			return;
		
		string name = GetUniqueName(attribute.Mask);
		
		if (string.IsNullOrEmpty(name))
			return;
		
		string path = $"{Path}/{name}";
		
		if (Data.Contains(path))
			return;
		
		AdminNode node = Data.CreateObject(this, path);
		
		if (node == null)
			return;
		
		node.Create();
	}

	public void Remove()
	{
		if (Parent == null || !Data.Contains(Path))
			return;
		
		Data.RemoveValue(Path);
		
		Parent.RemoveNode(this);
	}

	public abstract string Copy();

	public abstract void Paste(string _Value);

	public void Rename(string _Name)
	{
		string source = Path;
		string target = Path.Replace(Name, _Name);
		
		if (source == target || Data.Contains(target))
			return;
		
		Name = _Name;
		Path = target;
		
		object value = Data.GetValue(source);
		
		Data.RemoveValue(source);
		
		Data.SetValue(target, value);
		
		Rebind(source, target);
	}

	public bool HasAttribute<T>() where T : AdminAttribute => Data.HasAttribute<T>(Path);

	public T GetAttribute<T>() where T : AdminAttribute => Data.GetAttribute<T>(Path);

	public bool TryGetAttribute<T>(out T _Attribute) where T : AdminAttribute
	{
		_Attribute = GetAttribute<T>();
		
		return _Attribute != null;
	}

	public static AdminNode Parse(AdminData _Data, AdminNode _Node, string _Path)
	{
		string name = AdminUtility.GetName(_Path);
		
		AdminNode node = _Node.Children.FirstOrDefault(_Entry => _Entry.Name == name);
		
		if (node != null)
			return node;
		
		object value = _Data.GetValue(_Path);
		
		AdminNodeType type = AdminUtility.GetType(value);
		
		return Create(_Data, _Node, _Path, type);
	}

	public static AdminNode Create(AdminData _Data, AdminNode _Node, string _Path, AdminNodeType _Type)
	{
		switch (_Type)
		{
			case AdminNodeType.Number:  return new AdminNumberNode(_Data, _Node, _Path);
			case AdminNodeType.String:  return new AdminStringNode(_Data, _Node, _Path);
			case AdminNodeType.Boolean: return new AdminBooleanNode(_Data, _Node, _Path);
			case AdminNodeType.Object:  return new AdminObjectNode(_Data, _Node, _Path);
			case AdminNodeType.Array:   return new AdminArrayNode(_Data, _Node, _Path);
			default:                    return null;
		}
	}

	public AdminNode GetNode(string _Path)
	{
		string[] path = AdminUtility.GetPath(_Path);
		
		if (path == null || path.Length == 0)
			return null;
		
		AdminNode node = this;
		foreach (string entry in path)
		{
			if (node == null)
				break;
			
			node = node.Children.FirstOrDefault(_Node => _Node.Name == entry);
		}
		return node;
	}

	public void Attach(AdminNode _Node) => AddNode(_Node);

	protected void InvokeValueChanged() => m_ValueChanged.Invoke(Path);

	protected virtual void AddNode(AdminNode _Node)
	{
		m_Children.Add(_Node);
		_Node.Parent = this;
		_Node.Indent = Indent + 1;
		Reindent();
	}

	protected virtual void RemoveNode(AdminNode _Node)
	{
		m_Children.Remove(_Node);
		_Node.Parent = null;
	}

	void Reindent()
	{
		foreach (AdminNode node in Children)
		{
			node.Indent = Indent + 1;
			node.Reindent();
		}
	}

	void Rebind(string _Source, string _Target)
	{
		Path = Path.Replace(_Source, _Target);
		
		foreach (AdminNode node in Children)
			node.Rebind(_Source, _Target);
	}

	protected string GetUniqueName(string _Mask)
	{
		if (string.IsNullOrEmpty(_Mask))
			return _Mask;
		
		const int limit = 300;
		
		int count = Children.Count + 1;
		
		for (int i = 0; i < limit; i++)
		{
			string path = string.Format(_Mask, i + count);
			
			if (Data.Contains(path))
				continue;
			
			return path;
		}
		
		return string.Format(_Mask, CRC32.Get(Guid.NewGuid().ToString()));
	}
}
