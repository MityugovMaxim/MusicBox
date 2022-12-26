using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class DataListEntry : DataNode, IDataNodeEntry
{
	public override string Name
	{
		get
		{
			if (Parent is not DataListNode node)
				return base.Name;
			
			List<DataNode> nodes = node.Children;
			
			if (nodes == null || nodes.Count == 0)
				return base.Name;
			
			int index = node.Children.IndexOf(this);
			
			if (index < 0 || index >= nodes.Count)
				return base.Name;
			
			IList collection = base.GetValue<IList>();
			
			if (index >= collection.Count)
				return base.Name;
			
			return collection[index].ToString();
		}
	}

	public override Type Type => Property.PropertyType.GetGenericArguments().FirstOrDefault();

	public DataListEntry(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public override TValue GetValue<TValue>()
	{
		if (Parent is not DataListNode node)
			return default;
		
		List<DataNode> nodes = node.Children;
		
		if (nodes == null || nodes.Count == 0)
			return default;
		
		int index = node.Children.IndexOf(this);
		
		if (index < 0 || index >= nodes.Count)
			return default;
		
		IList collection = base.GetValue<IList>();
		
		if (index >= collection.Count)
			return default;
		
		return (TValue)collection[index];
	}

	public override void SetValue<TValue>(TValue _Value)
	{
		if (Parent is not DataListNode node)
			return;
		
		List<DataNode> nodes = node.Children;
		
		if (nodes == null || nodes.Count == 0)
			return;
		
		int index = node.Children.IndexOf(this);
		
		if (index < 0 || index >= nodes.Count)
			return;
		
		IList collection = base.GetValue<IList>();
		
		if (index >= collection.Count)
		{
			int delta = index - collection.Count + 1;
			for (int i = 0; i < delta; i++)
				collection.Add(default);
		}
		
		collection[index] = _Value;
	}

	public void MoveUp()
	{
		if (Parent is not DataListNode node)
			return;
		
		List<DataNode> nodes = node.Children;
		
		if (nodes == null || nodes.Count == 0)
			return;
		
		int source = nodes.IndexOf(this);
		int target = source - 1;
		
		if (source < 0 || source >= nodes.Count || target < 0 || target >= nodes.Count)
			return;
		
		(nodes[source], nodes[target]) = (nodes[target], nodes[source]);
		
		node.Rebuild();
	}

	public void MoveDown()
	{
		if (Parent is not DataListNode node)
			return;
		
		List<DataNode> nodes = node.Children;
		
		if (nodes == null || nodes.Count == 0)
			return;
		
		int source = nodes.IndexOf(this);
		int target = source + 1;
		
		if (source < 0 || source >= nodes.Count || target < 0 || target >= nodes.Count)
			return;
		
		(nodes[source], nodes[target]) = (nodes[target], nodes[source]);
		
		node.Rebuild();
	}

	public void Remove()
	{
		if (Parent is DataListNode node)
			node.Remove(this);
	}
}
