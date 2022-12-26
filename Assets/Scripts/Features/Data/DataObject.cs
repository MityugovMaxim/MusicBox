using System.Reflection;

public class DataObject : DataNode
{
	public DataObject(object _Target) : base(_Target, null, null)
	{
		ProcessObject();
	}

	void ProcessObject()
	{
		Clear();
		
		PropertyInfo[] properties = DataUtility.GetProperties(Target);
		
		if (properties == null || properties.Length == 0)
			return;
		
		foreach (PropertyInfo property in properties)
		{
			if (DataNodeFactory.TryCreate(Target, property, out DataNode node))
			{
				node.Initialize(this);
			}
		}
	}
}
