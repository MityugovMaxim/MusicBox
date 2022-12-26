using System;
using System.Collections;
using System.Reflection;

public class DataArrayEntry : DataNode, IDataNodeEntry
{
	int Index => Parent.FindIndex(this);

	public DataArrayEntry(object _Target, PropertyInfo _Property, IDataNodeParser _Parser) : base(_Target, _Property, _Parser) { }

	public override T GetValue<T>()
	{
		T[] array = base.GetValue<T[]>();
		
		if (array == null || array.Length == 0)
			return default;
		
		return array[Index];
	}

	public override void SetValue<T>(T _Value)
	{
		T[] array = base.GetValue<T[]>();
		
		if (array == null || array.Length == 0)
			return;
		
		array[Index] = _Value;
	}

	public void MoveUp()
	{
		Array array = GetValue<Array>();
		
		if (array == null || array.Length == 0)
			return;
		
		int index = Index - 1;
		
		if (index <= 0 || index >= array.Length)
			return;
		
		object buffer = array.GetValue(Index);
		array.SetValue(array.GetValue(index), Index);
		array.SetValue(buffer, index);
		
		SetValue(array);
	}

	public void MoveDown()
	{
		Array array = GetValue<Array>();
		
		if (array == null || array.Length == 0)
			return;
		
		int index = Index + 1;
		
		if (index <= 0 || index >= array.Length)
			return;
		
		object buffer = array.GetValue(Index);
		array.SetValue(array.GetValue(index), Index);
		array.SetValue(buffer, index);
		
		SetValue(array);
	}

	public void Remove()
	{
		Array array = GetValue<Array>();
		
		if (array == null || array.Length == 0)
			return;
		
		ArrayList list = new ArrayList(array);
		list.RemoveAt(Index);
		
		SetValue(list.ToArray());
		
		Remove(this);
	}
}
