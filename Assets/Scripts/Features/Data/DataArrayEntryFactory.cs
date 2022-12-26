using System;
using System.Reflection;

public class DataArrayEntryFactory
{
	public static bool TryCreate(object _Target, PropertyInfo _Property, out DataNode _Node)
	{
		_Node = null;
		
		if (_Target == null || _Property == null)
			return false;
		
		Type type = _Property.PropertyType.GetElementType();
		
		IDataNodeParser parser = DataUtility.GetParser(type);
		
		if (parser == null)
			return false;
		
		_Node = new DataArrayEntry(_Target, _Property, parser);
		
		return _Node != null;
	}
}
