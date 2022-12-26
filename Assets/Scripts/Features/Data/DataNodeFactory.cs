using System;
using System.Collections.Generic;
using System.Reflection;

public static class DataNodeFactory
{
	public static bool TryCreate(object _Target, PropertyInfo _Property, out DataNode _Node)
	{
		_Node = null;
		
		if (_Target == null || _Property == null)
			return false;
		
		Type type = _Property.PropertyType;
		
		IDataNodeParser parser = DataUtility.GetParser(type);
		
		if (type.IsArray)
			_Node = new DataArrayNode(_Target, _Property, parser);
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			_Node = new DataListNode(_Target, _Property, parser);
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			_Node = new DataDictionaryNode(_Target, _Property, parser);
		else
			_Node = new DataNode(_Target, _Property, parser);
		
		return true;
	}
}
