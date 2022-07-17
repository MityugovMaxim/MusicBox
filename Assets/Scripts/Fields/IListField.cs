using System;

public interface IListField
{
	object this[int _Index] { get; set; }

	Type EntryType { get; }

	void Remove(int _Index);

	void Modify(int _Index);
}