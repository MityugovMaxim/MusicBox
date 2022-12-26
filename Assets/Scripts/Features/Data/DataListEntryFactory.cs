using System;
using System.Linq;
using System.Reflection;

public static class DataListEntryFactory
{
	public static DataNode Create(DataNode _Node, object _Target, PropertyInfo _Property)
	{
		if (_Target == null || _Property == null)
			return null;
		
		Type type = _Property.PropertyType.GetGenericArguments().FirstOrDefault();
		
		IDataNodeParser parser = DataUtility.GetParser(type);
		
		if (parser == null)
			return null;
		
		DataListEntry node = new DataListEntry(_Target, _Property, parser);
		
		node.Initialize(_Node);
		
		return node;
	}
}
