using System.Collections;
using System.Reflection;

public class DataArrayNode : DataNode, IDataNodeCollection
{
	public int Length => Children.Count;

	public DataArrayNode(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public void Add()
	{
		if (DataArrayEntryFactory.TryCreate(Target, Property, out DataNode node))
		{
			node.Initialize(this);
		}
	}

	protected override void OnAdd(DataNode _DataNode)
	{
		ArrayList array = new ArrayList();
		
		foreach (DataNode node in Children)
			array.Add(DataUtility.GetValue(node));
		
		SetValue(array.ToArray());
	}

	protected override void OnRemove(DataNode _DataNode)
	{
		ArrayList array = new ArrayList();
		
		foreach (DataNode node in Children)
			array.Add(DataUtility.GetValue(node));
		
		SetValue(array.ToArray());
	}
}
