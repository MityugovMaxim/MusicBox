using System.Collections;
using System.Reflection;

public class DataListNode : DataNode, IDataNodeCollection
{
	public int Count => Children.Count;

	public DataListNode(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public void Add()
	{
		DataNode node = DataListEntryFactory.Create(this, Target, Property);
		
		InvokeChildAdded(node);
	}

	protected override void OnAdd(DataNode _DataNode) => Rebuild();

	protected override void OnRemove(DataNode _DataNode) => Rebuild();

	public void Rebuild()
	{
		IList list = GetValue<IList>();
		
		list.Clear();
		
		foreach (DataNode node in Children)
			list.Add(node.GetValue<object>());
		
		InvokeChildMoved();
	}
}
