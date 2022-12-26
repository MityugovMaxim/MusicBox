using System;
using System.Collections;
using System.Linq;
using System.Reflection;

public class DataDictionaryKey : DataNode
{
	public DataDictionaryKey(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public override void SetValue<T>(T _Value)
	{
		base.SetValue(_Value);
		
		IDictionary collection = Parent.GetValue<IDictionary>();
		
		T source = GetValue<T>();
		
		DataNode node = Parent.Children.LastOrDefault();
		
		object value = DataUtility.GetValue(node);
		
		collection.Remove(source);
		
		collection[_Value] = value;
	}
}

public class DataDictionaryValue : DataNode
{
	public DataDictionaryValue(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public override T GetValue<T>()
	{
		IDictionary collection = Parent.GetValue<IDictionary>();
		
		DataNode node = Parent.Children.FirstOrDefault();
		
		object key = DataUtility.GetValue(node);
		
		return (T)collection[key];
	}

	public override void SetValue<T>(T _Value)
	{
		base.SetValue(_Value);
		
		IDictionary collection = Parent.GetValue<IDictionary>();
		
		DataNode node = Parent.Children.FirstOrDefault();
		
		object key = DataUtility.GetValue(node);
		
		collection[key] = _Value;
	}
}

public class DataDictionaryEntry : DataNode
{
	public DataDictionaryEntry(object _Target, PropertyInfo _Property) : base(_Target, _Property, null) { }
}

public class DataDictionaryNode : DataNode, IDataNodeCollection
{
	public DataDictionaryNode(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public void Add()
	{
		
	}
}

public static class DataStructEntryFactory
{
	public static bool TryCreate(object _Target, PropertyInfo _Property, out DataNode _Node)
	{
		_Node = null;
		
		if (_Target == null || _Property == null)
			return false;
		
		Type type = _Property.PropertyType;
		
		IDataNodeParser parser = DataUtility.GetParser(type);
		
		if (parser == null)
			return false;
		
		_Node = new DataStructEntry(_Target, _Property, parser);
		
		return _Node != null;
	}
}

public class DataStructEntry : DataNode
{
	public DataStructEntry(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public override void SetValue<T>(T _Value)
	{
		ValueType target = Parent.GetValue<ValueType>();
		
		DataUtility.SetValue(Property, target, _Value);
		
		Parent.SetValue(target);
	}
}
