using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class DataAreaAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class DataDateAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class DataTickAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class DataSliderAttribute : Attribute
{
	public float Min   { get; }
	public float Max   { get; }
	public int   Steps { get; }

	public DataSliderAttribute(float _Min, float _Max, int _Steps = 0)
	{
		Min   = _Min;
		Max   = _Max;
		Steps = _Steps;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public class DataHideAttribute : Attribute { }

public interface IDataNodeCollection
{
	void Add();
}

public interface IDataNodeEntry
{
	void Remove();

	void MoveUp();

	void MoveDown();
}

public interface IDataNodeParser
{
	void Create(DataNode _DataNode);

	void Process(DataNode _DataNode);

	bool ValueEquals(object _Source, object _Target);

	string GetData(DataNode _DataNode);

	void SetData(DataNode _DataNode, string _Data);
}

public class DataIntegerParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is int source && _Target is int target)
			return source == target;
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<int>().ToString();

	public void SetData(DataNode _DataNode, string _Data)
	{
		if (!string.IsNullOrEmpty(_Data) && int.TryParse(_Data, out int value))
			_DataNode.SetValue(value);
	}
}

public class DataLongParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is long source && _Target is long target)
			return source == target;
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<long>().ToString();

	public void SetData(DataNode _DataNode, string _Data)
	{
		if (!string.IsNullOrEmpty(_Data) && long.TryParse(_Data, out long value))
			_DataNode.SetValue(value);
	}
}

public class DataFloatParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is float source && _Target is float target)
			return Mathf.Approximately(source, target);
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<float>().ToString(CultureInfo.InvariantCulture);

	public void SetData(DataNode _DataNode, string _Data)
	{
		if (!string.IsNullOrEmpty(_Data) && float.TryParse(_Data, out float value))
			_DataNode.SetValue(value);
	}
}

public class DataDoubleParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is double source && _Target is double target)
			return Math.Abs(source - target) < double.Epsilon * 2;
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<double>().ToString(CultureInfo.InvariantCulture);

	public void SetData(DataNode _DataNode, string _Data)
	{
		if (!string.IsNullOrEmpty(_Data) && double.TryParse(_Data, out double value))
			_DataNode.SetValue(value);
	}
}

public class DataDecimalParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is decimal source && _Target is decimal target)
			return source == target;
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<decimal>().ToString(CultureInfo.InvariantCulture);

	public void SetData(DataNode _DataNode, string _Data)
	{
		if (!string.IsNullOrEmpty(_Data) && decimal.TryParse(_Data, out decimal value))
			_DataNode.SetValue(value);
	}
}

public class DataBooleanParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is bool source && _Target is bool target)
			return source == target;
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<bool>().ToString();

	public void SetData(DataNode _DataNode, string _Data)
	{
		if (!string.IsNullOrEmpty(_Data) && bool.TryParse(_Data, out bool value))
			_DataNode.SetValue(value);
	}
}

public class DataDictionaryParser : IDataNodeParser
{
	public void Create(DataNode _DataNode)
	{
		IDictionary collection = _DataNode.GetValue<IDictionary>();
		
		if (collection != null)
			return;
		
		Type key   = _DataNode.Property.PropertyType.GetGenericArguments().FirstOrDefault();
		Type value = _DataNode.Property.PropertyType.GetGenericArguments().FirstOrDefault();
		Type type  = typeof(Dictionary<,>).MakeGenericType(key, value);
		
		collection = Activator.CreateInstance(type) as IDictionary;
		
		_DataNode.SetValue(collection);
	}

	public void Process(DataNode _DataNode)
	{
		_DataNode.Clear();
		
		IDictionary dictionary = _DataNode.GetValue<IDictionary>();
		
		foreach (IDictionaryEnumerator entry in dictionary)
		{
			DictionaryEntry target = entry.Entry;
			
			PropertyInfo keyProperty   = DataUtility.GetProperty(target, nameof(target.Key));
			PropertyInfo valueProperty = DataUtility.GetProperty(target, nameof(target.Value));
			
			DataNode node = new DataDictionaryEntry(_DataNode.Target, _DataNode.Property);
			
			if (DataNodeFactory.TryCreate(target, keyProperty, out DataNode keyNode))
				node.Add(keyNode);
			
			if (DataNodeFactory.TryCreate(target, valueProperty, out DataNode valueNode))
				node.Add(valueNode);
			
			_DataNode.Add(_DataNode);
		}
	}

	public bool ValueEquals(object _Source, object _Target) => _Source == _Target;

	public string GetData(DataNode _DataNode) => _DataNode.Property.PropertyType.Name;

	public void SetData(DataNode _DataNode, string _Data) { }
}

public class DataListParser : IDataNodeParser
{
	public void Create(DataNode _DataNode)
	{
		IList collection = _DataNode.GetValue<IList>();
		
		if (collection != null)
			return;
		
		Type entry = _DataNode.Property.PropertyType.GetGenericArguments().FirstOrDefault();
		Type type  = typeof(List<>).MakeGenericType(entry);
		
		collection = Activator.CreateInstance(type) as IList;
		
		_DataNode.SetValue(collection);
	}

	public void Process(DataNode _DataNode)
	{
		_DataNode.Clear();
		
		IList list = _DataNode.GetValue<IList>();
		
		if (list == null || list.Count == 0)
			return;
		
		for (int i = 0; i < list.Count; i++)
			DataListEntryFactory.Create(_DataNode, _DataNode.Target, _DataNode.Property);
	}

	public bool ValueEquals(object _Source, object _Target) => _Source == _Target;

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<IList>().ToString();

	public void SetData(DataNode _DataNode, string _Data) { }
}

public class DataArrayParser : IDataNodeParser
{
	public void Create(DataNode _DataNode)
	{
		Array collection = _DataNode.GetValue<Array>();
		
		if (collection != null)
			return;
		
		Type entry = _DataNode.Property.PropertyType.GetElementType();
		
		if (entry == null)
			return;
		
		Type type  = entry.MakeArrayType();
		
		collection = Activator.CreateInstance(type) as Array;
		
		_DataNode.SetValue(collection);
	}

	public void Process(DataNode _DataNode)
	{
		_DataNode.Clear();
		
		Array array = _DataNode.GetValue<Array>();
		
		if (array == null || array.Length == 0)
			return;
		
		for (int i = 0; i < array.Length; i++)
		{
			if (DataArrayEntryFactory.TryCreate(_DataNode.Target, _DataNode.Property, out DataNode node))
			{
				node.Initialize(_DataNode);
			}
		}
	}

	public bool ValueEquals(object _Source, object _Target) => _Source == _Target;

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<Array>().ToString();

	public void SetData(DataNode _DataNode, string _Data) { }
}

public class DataObjectParser : IDataNodeParser
{
	public void Create(DataNode _DataNode)
	{
		object value = _DataNode.GetValue<object>();
		
		if (value != null)
			return;
		
		Type type = _DataNode.Type;
		
		value = Activator.CreateInstance(type);
		
		_DataNode.SetValue(value);
	}

	public void Process(DataNode _DataNode)
	{
		_DataNode.Clear();
		
		object value = _DataNode.GetValue<object>();
		
		PropertyInfo[] properties = DataUtility.GetProperties(value);
		
		if (properties == null || properties.Length == 0)
			return;
		
		foreach (PropertyInfo property in properties)
		{
			if (DataNodeFactory.TryCreate(value, property, out DataNode node))
			{
				node.Initialize(_DataNode);
			}
		}
	}

	public bool ValueEquals(object _Source, object _Target)
	{
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.Name;

	public void SetData(DataNode _DataNode, string _Data) { }
}

public class DataStructParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode)
	{
		_DataNode.Clear();
		
		object value = _DataNode.GetValue<ValueType>();
		
		PropertyInfo[] properties = DataUtility.GetProperties(value);
		
		if (properties == null || properties.Length == 0)
			return;
		
		foreach (PropertyInfo property in properties)
		{
			if (DataStructEntryFactory.TryCreate(value, property, out DataNode node))
			{
				node.Initialize(_DataNode);
			}
		}
	}

	public bool ValueEquals(object _Source, object _Target) => _Source == _Target;

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<ValueType>().ToString();

	public void SetData(DataNode _DataNode, string _Data) { }
}

public class DataEnumParser : IDataNodeParser
{
	public void Create(DataNode _DataNode) { }

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is Enum source && _Target is Enum target)
			return Equals(source, target);
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<Enum>().ToString();

	public void SetData(DataNode _DataNode, string _Data)
	{
		if (!string.IsNullOrEmpty(_Data) && Enum.TryParse(_DataNode.Type, _Data, out object value))
			_DataNode.SetValue(value);
	}
}

public class DataStringParser : IDataNodeParser
{
	public void Create(DataNode _DataNode)
	{
		string value = _DataNode.GetValue<string>();
		
		if (value != null)
			return;
		
		value = string.Empty;
		
		_DataNode.SetValue(value);
	}

	public void Process(DataNode _DataNode) { }

	public bool ValueEquals(object _Source, object _Target)
	{
		if (_Source is string source && _Target is string target)
			return source == target;
		return _Source == _Target;
	}

	public string GetData(DataNode _DataNode) => _DataNode.GetValue<string>();

	public void SetData(DataNode _DataNode, string _Data) => _DataNode.SetValue(_Data);
}

public class DataNode : IEnumerable<DataNode>
{
	public virtual string Name
	{
		get
		{
			if (Property == null && Target == null)
				return "Unknown";
			
			if (Property == null && Target != null)
				return Target.ToString();
			
			return Property != null ? Property.Name : "null";
		}
	}

	public virtual Type Type
	{
		get
		{
			if (Property != null)
				return Property.PropertyType;
			
			if (Target != null)
				return Target.GetType();
			
			return null;
		}
	}

	public DataNode this[string _Path]
	{
		get
		{
			if (string.IsNullOrEmpty(_Path))
				return null;
			
			string[] path = _Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
			
			DataNode node = this;
			foreach (string name in path)
			foreach (DataNode child in node.Children)
			{
				if (child == null || child.Name != name)
					continue;
				
				node = child;
				
				break;
			}
			
			return node;
		}
	}

	public object         Target   { get; }
	public PropertyInfo   Property { get; }
	public DataNode       Parent   { get; private set; }
	public List<DataNode> Children => m_Children;
	public int            Level    { get; private set; }

	public readonly DynamicDelegate           ValueChanged = new DynamicDelegate();
	public readonly DynamicDelegate<DataNode> ChildAdded   = new DynamicDelegate<DataNode>();
	public readonly DynamicDelegate<DataNode> ChildRemoved = new DynamicDelegate<DataNode>();
	public readonly DynamicDelegate           ChildMoved   = new DynamicDelegate();

	readonly List<DataNode>  m_Children = new List<DataNode>();
	readonly IDataNodeParser m_Parser;

	public DataNode(object _Target, PropertyInfo _Property, IDataNodeParser _Parser)
	{
		Target   = _Target;
		Property = _Property;
		m_Parser = _Parser;
	}

	public void Initialize(DataNode _DataNode)
	{
		Parent = _DataNode;
		Level  = Parent.Level + 1;
		Parent?.m_Children.Add(this);
		m_Parser?.Create(this);
		m_Parser?.Process(this);
	}

	public int FindIndex(DataNode _DataNode)
	{
		return m_Children.IndexOf(_DataNode);
	}

	public void Add(DataNode _DataNode)
	{
		if (_DataNode == null)
			return;
		
		Children.Add(_DataNode);
		
		_DataNode.Parent = this;
		_DataNode.Level  = Level + 1;
		
		OnAdd(_DataNode);
		
		ChildAdded?.Invoke(_DataNode);
	}

	public void Remove(DataNode _DataNode)
	{
		if (_DataNode == null || !Children.Contains(_DataNode))
			return;
		
		Children.Remove(_DataNode);
		
		_DataNode.Parent = null;
		_DataNode.Level  = 0;
		
		OnRemove(_DataNode);
		
		ChildRemoved?.Invoke(_DataNode);
	}

	public void Clear()
	{
		List<DataNode> children = new List<DataNode>(m_Children);
		
		m_Children.Clear();
		
		foreach (DataNode node in children)
		{
			node.Parent = null;
			node.Level  = 0;
			
			ChildRemoved?.Invoke(node);
			
			OnRemove(node);
		}
	}

	public bool HasAttribute<T>() where T : Attribute
	{
		return Property?.GetCustomAttribute<T>() != null;
	}

	public bool TryGetAttribute<T>(out T _Attribute) where T : Attribute
	{
		_Attribute = Property.GetCustomAttribute<T>();
		
		return _Attribute != null;
	}

	public virtual T GetValue<T>()
	{
		if (Property == null && Target == null)
			return default;
		
		if (Property == null && Target != null)
			return (T)Target;
		
		if (Property == null)
			return default;
		
		return typeof(T) == Property.PropertyType
			? DataUtility.GetValue<T>(Property, Target)
			: DataUtility.GetCastValue<T>(Property, Target);
	}

	public virtual void SetValue<T>(T _Value)
	{
		T value = GetValue<T>();
		
		if (ValueEquals(value, _Value))
			return;
		
		if (typeof(T) == Property.PropertyType)
			DataUtility.SetValue(Property, Target, _Value);
		else
			DataUtility.SetCastValue(Property, Target, _Value);
		
		ValueChanged?.Invoke();
	}

	bool ValueEquals(object _Source, object _Target) => m_Parser?.ValueEquals(_Source, _Target) ?? _Source == _Target;

	public string GetData()
	{
		if (m_Parser != null)
			return m_Parser.GetData(this);
		object data = DataUtility.GetValue(Property, Target);
		return data != null ? data.ToString() : "null";
	}

	public void SetData(string _Data) => m_Parser?.SetData(this, _Data);

	public IEnumerator<DataNode> GetEnumerator() => Children.GetEnumerator();

	protected virtual void OnAdd(DataNode _DataNode) { }

	protected virtual void OnRemove(DataNode _DataNode) { }

	protected void InvokeChildAdded(DataNode _DataNode) => ChildAdded?.Invoke(_DataNode);

	protected void InvokeChildRemoved(DataNode _DataNode) => ChildRemoved?.Invoke(_DataNode);

	protected void InvokeChildMoved() => ChildMoved?.Invoke();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
