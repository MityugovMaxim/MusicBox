using System;
using System.Reflection;

public class DataObjectNode : DataNode
{
	public DataObjectNode(object _Target, PropertyInfo _Property) : base(_Target, _Property, null)
	{
		ProcessObject();
	}

	void ProcessObject()
	{
		Clear();
		
		object value = GetValue<object>();
		
		if (value == null)
			value = Activator.CreateInstance(Property.PropertyType);
		
		SetValue(value);
		
		PropertyInfo[] properties = DataUtility.GetProperties(value);
		
		if (properties == null || properties.Length == 0)
			return;
		
		foreach (PropertyInfo property in properties)
		{
			if (DataNodeFactory.TryCreate(value, property, out DataNode node))
			{
				node.Initialize(this);
			}
		}
	}
}
